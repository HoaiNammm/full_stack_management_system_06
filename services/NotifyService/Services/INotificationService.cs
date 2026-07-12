using NotifyService.DTOs.Notifications;

namespace NotifyService.Services;

public interface INotificationService
{
    Task<List<NotificationDto>> GetByUserAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task MarkReadAsync(Guid userNotificationId, Guid userId);
    Task MarkAllReadAsync(Guid userId);
    Task CreateAsync(CreateNotificationRequest request);
}
