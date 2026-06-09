using Microsoft.EntityFrameworkCore;
using NotifyService.Api.Data;
using NotifyService.Api.Models;

namespace NotifyService.Api.Services;

public class NotificationService
{
    private readonly AppDbContext _context;

    public NotificationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Guid userId, string title, string content, string type,
        Guid? relatedTaskId = null, Guid? relatedProjectId = null)
    {
        // Kiểm tra preference trước khi tạo
        var pref = await _context.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId);
        if (pref != null && !ShouldSend(pref, type)) return;

        _context.Notifications.Add(new Notification
        {
            Id               = Guid.NewGuid(),
            UserId           = userId,
            Title            = title,
            Content          = content,
            Type             = type,
            RelatedTaskId    = relatedTaskId,
            RelatedProjectId = relatedProjectId,
            IsRead           = false,
            CreatedAt        = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    public async Task<List<Notification>> GetForUserAsync(Guid userId, bool unreadOnly = false)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId);

        if (unreadOnly) query = query.Where(n => !n.IsRead);

        return await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId) =>
        await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

    public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
        if (notification == null) return false;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        var now = DateTime.UtcNow;
        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = now;
        }

        await _context.SaveChangesAsync();
    }

    private static bool ShouldSend(UserPreference pref, string type) => type switch
    {
        "task_assigned" or "task_column_changed" => pref.EnableTaskNotification,
        "comment_mention"                        => pref.EnableMentionNotification,
        "member_added" or "sprint_started"       => pref.EnableTaskNotification,
        _                                        => pref.EnableCommentNotification
    };
}
