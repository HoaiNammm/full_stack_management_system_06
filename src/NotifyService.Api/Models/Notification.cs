namespace NotifyService.Api.Models;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? TaskId { get; set; }

    public Guid? ProjectId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string? SourceService { get; set; }

    public string? SourceEventId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
    public ICollection<NotificationLog> NotificationLogs { get; set; } = new List<NotificationLog>();
}