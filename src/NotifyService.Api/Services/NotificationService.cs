using Microsoft.EntityFrameworkCore;
using NotifyService.Api.Data;
using NotifyService.Api.Models;

namespace NotifyService.Api.Services;

public class NotificationService
{
    private readonly NotifyDbContext _context;

    public NotificationService(NotifyDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Guid userId, string title, string content, string type,
        Guid? relatedTaskId = null, Guid? relatedProjectId = null)
    {
        var notification = new Notification
        {
            Id            = Guid.NewGuid(),
            TaskId        = relatedTaskId,
            ProjectId     = relatedProjectId,
            Title         = title,
            Message       = content,
            Type          = type,
            SourceService = "NotifyService",
            CreatedAt     = DateTime.UtcNow
        };

        notification.UserNotifications.Add(new UserNotification
        {
            Id             = Guid.NewGuid(),
            NotificationId = notification.Id,
            UserId         = userId,
            IsRead         = false,
            CreatedAt      = DateTime.UtcNow
        });

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Notification>> GetForUserAsync(Guid userId, bool unreadOnly = false)
    {
        var query = _context.UserNotifications
            .Include(un => un.Notification)
            .Where(un => un.UserId == userId);

        if (unreadOnly) query = query.Where(un => !un.IsRead);

        var userNotifications = await query
            .OrderByDescending(un => un.CreatedAt)
            .ToListAsync();

        return userNotifications
            .Where(un => un.Notification != null)
            .Select(un => un.Notification!)
            .ToList();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId) =>
        await _context.UserNotifications.CountAsync(un => un.UserId == userId && !un.IsRead);

    public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var userNotification = await _context.UserNotifications
            .FirstOrDefaultAsync(un => un.NotificationId == notificationId && un.UserId == userId);
        if (userNotification == null) return false;

        userNotification.IsRead = true;
        userNotification.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var unread = await _context.UserNotifications
            .Where(un => un.UserId == userId && !un.IsRead)
            .ToListAsync();

        var now = DateTime.UtcNow;
        foreach (var un in unread)
        {
            un.IsRead = true;
            un.ReadAt = now;
        }

        await _context.SaveChangesAsync();
    }
}
