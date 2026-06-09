using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotifyService.Api.Data;

namespace NotifyService.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly NotifyDbContext _context;

    public NotificationsController(NotifyDbContext context)
    {
        _context = context;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu UserId" });
        }

        var notifications = await _context.UserNotifications
            .Include(x => x.Notification)
            .Where(x => x.UserId == userId.Value)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                userNotificationId = x.Id,
                notificationId = x.NotificationId,
                x.UserId,
                x.IsRead,
                x.ReadAt,
                x.CreatedAt,
                title = x.Notification != null ? x.Notification.Title : "",
                message = x.Notification != null ? x.Notification.Message : "",
                type = x.Notification != null ? x.Notification.Type : "",
                taskId = x.Notification != null ? x.Notification.TaskId : null,
                projectId = x.Notification != null ? x.Notification.ProjectId : null
            })
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu UserId" });
        }

        var count = await _context.UserNotifications
            .CountAsync(x => x.UserId == userId.Value && !x.IsRead);

        return Ok(new
        {
            unreadCount = count
        });
    }

    [HttpPut("{userNotificationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid userNotificationId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu UserId" });
        }

        var userNotification = await _context.UserNotifications
            .FirstOrDefaultAsync(x => x.Id == userNotificationId && x.UserId == userId.Value);

        if (userNotification == null)
        {
            return NotFound(new { message = "Không tìm thấy thông báo" });
        }

        if (!userNotification.IsRead)
        {
            userNotification.IsRead = true;
            userNotification.ReadAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        return Ok(new
        {
            message = "Đã đánh dấu thông báo là đã đọc",
            userNotificationId = userNotification.Id
        });
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu UserId" });
        }

        var unreadNotifications = await _context.UserNotifications
            .Where(x => x.UserId == userId.Value && !x.IsRead)
            .ToListAsync();

        foreach (var item in unreadNotifications)
        {
            item.IsRead = true;
            item.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Đã đánh dấu tất cả thông báo là đã đọc",
            updatedCount = unreadNotifications.Count
        });
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            return null;
        }

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }
}