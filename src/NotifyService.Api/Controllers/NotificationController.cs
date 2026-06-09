using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotifyService.Api.Services;

namespace NotifyService.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationController : ControllerBase
{
    private readonly NotificationService _notificationService;

    public NotificationController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications([FromQuery] bool unreadOnly = false)
    {
        var userId        = Guid.Parse(User.FindFirst("sub")!.Value);
        var notifications = await _notificationService.GetForUserAsync(userId, unreadOnly);
        return Ok(new { success = true, data = notifications });
    }

    [Authorize]
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        var count  = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(new { success = true, data = new { count } });
    }

    [Authorize]
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId  = Guid.Parse(User.FindFirst("sub")!.Value);
        var success = await _notificationService.MarkAsReadAsync(id, userId);

        if (!success)
            return NotFound(new { success = false, error = new { code = "NOTIFICATION_NOT_FOUND" } });

        return Ok(new { success = true });
    }

    [Authorize]
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        await _notificationService.MarkAllAsReadAsync(userId);
        return Ok(new { success = true });
    }
}
