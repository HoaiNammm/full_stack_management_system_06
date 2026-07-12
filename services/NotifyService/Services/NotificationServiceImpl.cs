using Microsoft.EntityFrameworkCore;
using NotifyService.Data;
using NotifyService.DTOs.Notifications;
using NotifyService.Models;

namespace NotifyService.Services;

public class NotificationServiceImpl : INotificationService
{
    private readonly NotifyDbContext _db;

    public NotificationServiceImpl(NotifyDbContext db) => _db = db;

    public async Task<List<NotificationDto>> GetByUserAsync(Guid userId)
    {
        return await _db.UserNotifications
            .Where(un => un.UserId == userId)
            .Include(un => un.Notification)
            .OrderByDescending(un => un.Notification!.CreatedAt)
            .Take(50)
            .Select(un => new NotificationDto
            {
                UserNotificationId = un.Id,
                NotificationId     = un.NotificationId,
                Type               = un.Notification!.Type,
                Title              = un.Notification.Title,
                Message            = un.Notification.Message,
                RelatedTaskId      = un.Notification.RelatedTaskId,
                RelatedProjectId   = un.Notification.RelatedProjectId,
                IsRead             = un.IsRead,
                CreatedAt          = un.Notification.CreatedAt,
            })
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
        => await _db.UserNotifications.CountAsync(un => un.UserId == userId && !un.IsRead);

    public async Task MarkReadAsync(Guid userNotificationId, Guid userId)
    {
        var un = await _db.UserNotifications
            .FirstOrDefaultAsync(u => u.Id == userNotificationId && u.UserId == userId);
        if (un is null) return;
        un.IsRead = true;
        await _db.SaveChangesAsync();
    }

    public async Task MarkAllReadAsync(Guid userId)
    {
        var unread = await _db.UserNotifications
            .Where(u => u.UserId == userId && !u.IsRead)
            .ToListAsync();
        unread.ForEach(u => u.IsRead = true);
        await _db.SaveChangesAsync();
    }

    public async Task CreateAsync(CreateNotificationRequest request)
    {
        if (!request.RecipientIds.Any()) return;

        var notification = new Notification
        {
            Type             = request.Type,
            Title            = request.Title,
            Message          = request.Message,
            RelatedTaskId    = request.RelatedTaskId,
            RelatedProjectId = request.RelatedProjectId,
        };
        _db.Notifications.Add(notification);

        foreach (var uid in request.RecipientIds.Distinct())
        {
            _db.UserNotifications.Add(new UserNotification
            {
                UserId         = uid,
                NotificationId = notification.Id,
            });
        }

        await _db.SaveChangesAsync();
    }
}
