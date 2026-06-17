using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotifyService.Api.DTOs;
using NotifyService.Api.Services;
using NotifyService.Api.DTOs;

namespace NotifyService.Api.Controllers;

[ApiController]
[Route("api/comments")]
public class CommentController : ControllerBase
{
    private readonly CommentService _commentService;

    public CommentController(CommentService commentService)
    {
        _commentService = commentService;
    }

    [Authorize]
    [HttpGet("task/{taskId}")]
    public async Task<IActionResult> GetByTask(Guid taskId)
    {
        var comments = await _commentService.GetByTaskAsync(taskId);
        return Ok(new { success = true, data = comments });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCommentRequest request)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);

        var comment = await _commentService.CreateAsync(
            taskId:          request.TaskId,
            authorId:        userId,
            content:         request.Content,
            mentionedUserIds: request.MentionedUserIds ?? new List<Guid>(),
            taskAssigneeId:  request.TaskAssigneeId);

        return Ok(new { success = true, data = comment });
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCommentRequest request)
    {
        var userId  = Guid.Parse(User.FindFirst("sub")!.Value);
        var comment = await _commentService.UpdateAsync(id, userId, request.Content);

        if (comment == null)
            return NotFound(new { success = false, error = new { code = "COMMENT_NOT_FOUND" } });

        return Ok(new { success = true, data = comment });
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId  = Guid.Parse(User.FindFirst("sub")!.Value);
        var success = await _commentService.DeleteAsync(id, userId);

        if (!success)
            return NotFound(new { success = false, error = new { code = "COMMENT_NOT_FOUND" } });

        return Ok(new { success = true });
    }

    
}
