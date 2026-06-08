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
        var mentionedUserIds = request.MentionedUserIds
            .Where(x => x != Guid.Empty && x != userId)
            .Distinct()
            .ToList();
        //  Xu li mentionedUserIds
        foreach (var mentionedUserId in mentionedUserIds)
        {
            comment.Mentions.Add(new CommentMention
            {
                Id = Guid.NewGuid(),
                CommentId = comment.Id,
                MentionedUserId = mentionedUserId,
                CreatedAt = DateTime.UtcNow
            });
        }
        // Xu ly actachment
        foreach (var attachment in request.Attachments)
        {
            if (string.IsNullOrWhiteSpace(attachment.FileName) ||
                string.IsNullOrWhiteSpace(attachment.FileUrl))
            {
                continue;
            }

            comment.Attachments.Add(new CommentAttachment
            {
                Id = Guid.NewGuid(),
                CommentId = comment.Id,
                FileName = attachment.FileName.Trim(),
                FileUrl = attachment.FileUrl.Trim(),
                ContentType = attachment.ContentType,
                FileSize = attachment.FileSize,
                CreatedAt = DateTime.UtcNow
            });
        }

        _context.Comments.Add(comment);
        AddActivityLog(
            userId: userId,
            taskId: comment.TaskId,
            projectId: comment.ProjectId,
            action: "COMMENT_CREATED",
            description: "User created a comment on task",
            metadataJson: $"{{\"commentId\":\"{comment.Id}\"}}"

        );
        AddAuditLog(
            userId: userId,
            action: "CREATE",
            entityName: "Comment",
            entityId: comment.Id.ToString(),
            oldValuesJson: null,
            newValuesJson: $"{{\"content\":\"{comment.Content}\"}}"
        );

        // goi helper khi tao comment

        CreateMentionNotifications(
            comment: comment,
            actorUserId: userId,
            mentionedUserIds: mentionedUserIds
        );
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
            UpdatedAt = comment.UpdatedAt,
            MentionedUserIds = mentionedUserIds,
            Attachments = comment.Attachments.Select(a => new CommentAttachmentResponse
            {
                Id = a.Id,
                CommentId = a.CommentId,
                FileName = a.FileName,
                FileUrl = a.FileUrl,
                ContentType = a.ContentType,
                FileSize = a.FileSize,
                CreatedAt = a.CreatedAt
            }).ToList()
        };

        return Ok(response);
    }
    // Lấy danh sách comment
    [HttpGet("task/{taskId:guid}")]
    public async Task<IActionResult> GetCommentsByTask(Guid taskId)
    {
        var comments = await _context.Comments
    .Include(x => x.Mentions)
    .Include(x => x.Attachments)
    .Where(x => x.TaskId == taskId && !x.IsDeleted)
    .OrderBy(x => x.CreatedAt)
    .Select(x => new CommentResponse
    {
        Id = x.Id,
        TaskId = x.TaskId,
        ProjectId = x.ProjectId,
        UserId = x.UserId,
        Content = x.Content,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
        MentionedUserIds = x.Mentions
            .Select(m => m.MentionedUserId)
            .ToList(),
        Attachments = x.Attachments
            .Select(a => new CommentAttachmentResponse
            {
                Id = a.Id,
                CommentId = a.CommentId,
                FileName = a.FileName,
                FileUrl = a.FileUrl,
                ContentType = a.ContentType,
                FileSize = a.FileSize,
                CreatedAt = a.CreatedAt
            })
            .ToList()
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

        var oldContent = comment.Content;

        comment.Content = request.Content.Trim();
        comment.UpdatedAt = DateTime.UtcNow;

        AddAuditLog(
            userId: userId,
            action: "UPDATE",
            entityName: "Comment",
            entityId: comment.Id.ToString(),
            oldValuesJson: $"{{\"content\":\"{oldContent}\"}}",
            newValuesJson: $"{{\"content\":\"{comment.Content}\"}}"
        );

        comment.Content = request.Content.Trim();
        comment.UpdatedAt = DateTime.UtcNow; // Cập nhật thời gian sửa
        AddActivityLog(
            userId: userId,
            taskId: comment.TaskId,
            projectId: comment.ProjectId,
            action: "COMMENT_UPDATED",
            description: "User updated a comment on task",
            metadataJson: $"{{\"commentId\":\"{comment.Id}\"}}"
        );
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


    [HttpDelete("{commentId:guid}")]
    public async Task<IActionResult> DeleteComment(Guid commentId)
    {
        // Lấy user id từ payload jwt  (json web token)
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu UserId" });
        }
        //  Tìm comment theo id bỏ qua cac comment bị xóa
        var comment = await _context.Comments
            .FirstOrDefaultAsync(x => x.Id == commentId && !x.IsDeleted);

        if (comment == null)
        {
            return NotFound(new { message = "Không tìm thấy bình luận" });
        }
        //  Chặn xóa comment của người khác
        if (comment.UserId != userId)
        {
            return Forbid();
        }

        comment.IsDeleted = true;
        comment.UpdatedAt = DateTime.UtcNow;
        AddActivityLog(
            userId: userId,
            taskId: comment.TaskId,
            projectId: comment.ProjectId,
            action: "COMMENT_DELETED",
            description: "User deleted a comment on task",
            metadataJson: $"{{\"commentId\":\"{comment.Id}\"}}"
        );

        AddAuditLog(
            userId: userId,
            action: "SOFT_DELETE",
            entityName: "Comment",
            entityId: comment.Id.ToString(),
            oldValuesJson: "{\"isDeleted\":false}",
            newValuesJson: "{\"isDeleted\":true}"
        );

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Xóa bình luận thành công",
            commentId = comment.Id
        });

    }

    //Nhat ki hoat dong tai/sua xoa comment
    private void AddActivityLog(
    Guid userId,
    Guid taskId,
    Guid? projectId,
    string action,
    string description,
    string? metadataJson = null)
    {
        var activityLog = new ActivityLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TaskId = taskId,
            ProjectId = projectId,
            Action = action,
            Description = description,
            MetadataJson = metadataJson,
            CreatedAt = DateTime.UtcNow
        };

        _context.ActivityLogs.Add(activityLog);
    }

    // Notifications
    // 1. Tạo 1 bản ghi trong Notifications
    // 2. Tạo nhiều bản ghi trong UserNotifications cho từng người được mention
    // 3. Tạo NotificationLog để ghi lại việc tạo thông báo
    private void CreateMentionNotifications(Comment comment, Guid actorUserId, List<Guid> mentionedUserIds)
    {
        if (mentionedUserIds.Count == 0)
        {
            return;
        }

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            TaskId = comment.TaskId,
            ProjectId = comment.ProjectId,
            Title = "Bạn được nhắc đến trong một bình luận",
            Message = "Có người đã nhắc đến bạn trong một bình luận.",
            Type = "COMMENT_MENTION",
            SourceService = "NotifyService",
            SourceEventId = comment.Id.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        foreach (var mentionedUserId in mentionedUserIds)
        {
            notification.UserNotifications.Add(new UserNotification
            {
                Id = Guid.NewGuid(),
                NotificationId = notification.Id,
                UserId = mentionedUserId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        notification.NotificationLogs.Add(new NotificationLog
        {
            Id = Guid.NewGuid(),
            NotificationId = notification.Id,
            UserId = actorUserId,
            Action = "NOTIFICATION_CREATED",
            Status = "Success",
            ErrorMessage = null,
            CreatedAt = DateTime.UtcNow
        });

        _context.Notifications.Add(notification);
    }

    // Ghi log tao sau khi tao  comment
    private void AddAuditLog(

    Guid? userId,
    string action,
    string entityName,
    string? entityId,
    string? oldValuesJson = null,
    string? newValuesJson = null)

    {
        _context.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValuesJson = oldValuesJson,
            NewValuesJson = newValuesJson,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            CreatedAt = DateTime.UtcNow
        });


    }




}