namespace NotifyService.Models;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty;      // comment_mention, task_assigned, member_added
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? RelatedTaskId { get; set; }
    public Guid? RelatedProjectId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<UserNotification> UserNotifications { get; set; } = new();
}

public class UserNotification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid NotificationId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Notification? Notification { get; set; }
}
