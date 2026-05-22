using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotifyService.Api.Data;
using NotifyService.Api.DTOs;
using NotifyService.Api.Models;

namespace NotifyService.Api.Controllers;

[ApiController]
[Route("api/comments")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly NotifyDbContext _context;

    public CommentsController(NotifyDbContext context)
    {
        _context = context;
    }
    //  CreateComment
    // Kiểm tra dữ liệu đầu vào  TaskId rỗng  & content trống -> trả về lỗi 400
    [HttpPost]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request)
    {
        if (request.TaskId == Guid.Empty)
        {
            return BadRequest(new { message = "TaskId không hợp lệ" });
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest(new { message = "Nội dung bình luận không được để trống" });
        }
        //  Trích xuất user đăng nhập
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu UserId" });
        }
        // Khởi tạo Object coment mới , gán id, gán hười gian tạo

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            TaskId = request.TaskId,
            ProjectId = request.ProjectId,
            UserId = userId,
            Content = request.Content.Trim(),
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        // Lưu vào database
        await _context.SaveChangesAsync();

        var response = new CommentResponse
        {
            Id = comment.Id,
            TaskId = comment.TaskId,
            ProjectId = comment.ProjectId,
            UserId = comment.UserId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };

        return Ok(response);
    }
    // Lấy danh sách comment
    [HttpGet("task/{taskId:guid}")]
    public async Task<IActionResult> GetCommentsByTask(Guid taskId)
    {
        var comments = await _context.Comments
            .Where(x => x.TaskId == taskId && !x.IsDeleted) //Lọc comment theo taskId và loại bỏ comment đã xóa
            .OrderBy(x => x.CreatedAt)// Sắp xếp comment theo thời gian tạo
            .Select(x => new CommentResponse// Mapig dữ liệu từ DB(Comment qua CommentReponse )
            {
                Id = x.Id,
                TaskId = x.TaskId,
                ProjectId = x.ProjectId,
                UserId = x.UserId,
                Content = x.Content,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return Ok(comments);
    }
    [HttpPut("{commentId:guid}")]
    public async Task<IActionResult> UpdateComment(Guid commentId, [FromBody] UpdateCommentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))//Nội dung rỗng  trả về 400
        {
            return BadRequest(new { message = "Nội dung bình luận không được để trống" });
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu UserId" });
        }

        var comment = await _context.Comments // tìm comment mang id = commentId
            .FirstOrDefaultAsync(x => x.Id == commentId && !x.IsDeleted);

        if (comment == null)// không tìm thấy comment trả về null = 404
        {
            return NotFound(new { message = "Không tìm thấy bình luận" });
        }

        if (comment.UserId != userId) //Kiểm tra id check chủ comments
        {
            return Forbid();
        }

        comment.Content = request.Content.Trim();
        comment.UpdatedAt = DateTime.UtcNow; // Cập nhật thời gian sửa
                                             // Lưu database
        await _context.SaveChangesAsync();

        return Ok(new CommentResponse
        {
            Id = comment.Id,
            TaskId = comment.TaskId,
            ProjectId = comment.ProjectId,
            UserId = comment.UserId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        });
    }


}