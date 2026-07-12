namespace TaskService.DTOs.ActivityLogs;

public class ActivityLogDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? EntityName { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public ActorInfo? Actor { get; set; }
}

public class ActorInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}
