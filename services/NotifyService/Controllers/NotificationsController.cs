using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotifyService.DTOs.Notifications;
using NotifyService.Services;

namespace NotifyService.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
        => _notificationService = notificationService;

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _notificationService.GetByUserAsync(CurrentUserId));

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
        => Ok(new { unreadCount = await _notificationService.GetUnreadCountAsync(CurrentUserId) });

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        await _notificationService.MarkReadAsync(id, CurrentUserId);
        return NoContent();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        await _notificationService.MarkAllReadAsync(CurrentUserId);
        return NoContent();
    }
}

// Internal endpoint — called by TaskService / ProjectService (no JWT required)
[ApiController]
[Route("internal/notifications")]
public class NotificationsInternalController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsInternalController(INotificationService notificationService)
        => _notificationService = notificationService;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNotificationRequest request)
    {
        await _notificationService.CreateAsync(request);
        return Ok();
    }
}
