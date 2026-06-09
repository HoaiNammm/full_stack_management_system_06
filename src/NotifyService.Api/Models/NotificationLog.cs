namespace NotifyService.Api.Models;

public class NotificationLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid NotificationId { get; set; }

    public Guid? UserId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Notification? Notification { get; set; }
}