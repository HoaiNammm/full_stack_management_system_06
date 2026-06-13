namespace NotifyService.Api.Models;

public class ActivityLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty; // created_task / updated_task / moved_task / commented / assigned_task
    public string Description { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty; // Task / Project / Comment
    public Guid ResourceId { get; set; }
    public Guid? TaskId { get; set; }
    public Guid? ProjectId { get; set; }
    public string? MetadataJson { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
