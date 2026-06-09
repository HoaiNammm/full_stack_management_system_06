namespace NotifyService.Api.Models;

public class ActivityLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty; // created_task / updated_task / moved_task / commented / assigned_task
    public string ResourceType { get; set; } = string.Empty; // Task / Project / Comment
    public Guid ResourceId { get; set; }
    public DateTime Timestamp { get; set; }

    public User User { get; set; } = null!;
}
