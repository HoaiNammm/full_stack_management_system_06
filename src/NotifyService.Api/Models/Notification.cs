namespace NotifyService.Api.Models;

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // task_assigned / task_column_changed / comment_mention / member_added / sprint_started
    public Guid? RelatedTaskId { get; set; }     // logical ref → TaskDB
    public Guid? RelatedProjectId { get; set; }  // logical ref → ProjectDB
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
