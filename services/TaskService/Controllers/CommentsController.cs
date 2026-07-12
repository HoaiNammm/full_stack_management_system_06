using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskService.DTOs.Comments;
using TaskService.Services;

namespace TaskService.Controllers;

[ApiController]
[Route("api/tasks/{taskId:guid}/comments")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService) => _commentService = commentService;

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid taskId) =>
        Ok(await _commentService.GetByTaskAsync(taskId, CurrentUserId));

    [HttpPost]
    public async Task<IActionResult> Create(Guid taskId, [FromBody] CreateCommentRequest request)
    {
        var comment = await _commentService.CreateAsync(taskId, request, CurrentUserId);
        return Ok(comment);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid taskId, Guid id, [FromBody] UpdateCommentRequest request) =>
        Ok(await _commentService.UpdateAsync(id, request, CurrentUserId));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid taskId, Guid id)
    {
        await _commentService.DeleteAsync(id, CurrentUserId);
        return NoContent();
    }
}
