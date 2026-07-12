namespace NotifyService.DTOs.Notifications;

public class NotificationDto
{
    public Guid UserNotificationId { get; set; }
    public Guid NotificationId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? RelatedTaskId { get; set; }
    public Guid? RelatedProjectId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UnreadCountDto
{
    public int UnreadCount { get; set; }
}

public class CreateNotificationRequest
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<Guid> RecipientIds { get; set; } = new();
    public Guid? RelatedTaskId { get; set; }
    public Guid? RelatedProjectId { get; set; }
}
