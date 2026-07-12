using System.ComponentModel.DataAnnotations;

namespace TaskService.DTOs.Tasks;

public class TaskDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? SprintId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public List<string> Labels { get; set; } = new();
    public Guid? AssigneeId { get; set; }
    public AssigneeInfo? Assignee { get; set; }
    public DateTime? Deadline { get; set; }
    public decimal? EstimatedHours { get; set; }
    public Guid CreatedById { get; set; }
    public int SubTaskCount { get; set; }
    public int CompletedSubTaskCount { get; set; }
    public decimal TotalHoursLogged { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AssigneeInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
}

public class CreateTaskRequest
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public string Status { get; set; } = "Backlog";
    public string Type { get; set; } = "Task";
    public string Priority { get; set; } = "Medium";
    public List<string> Labels { get; set; } = new();
    public Guid? AssigneeId { get; set; }
    public DateTime? Deadline { get; set; }
    public Guid? SprintId { get; set; }
    public decimal? EstimatedHours { get; set; }
}

public class UpdateTaskRequest
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public string? Status { get; set; }
    public string? Type { get; set; }
    public string? Priority { get; set; }
    public List<string>? Labels { get; set; }
    public Guid? AssigneeId { get; set; }
    public bool ClearAssignee { get; set; } = false;
    public DateTime? Deadline { get; set; }
    public bool ClearDeadline { get; set; } = false;
    public Guid? SprintId { get; set; }
    public bool ClearSprint { get; set; } = false;
    public decimal? EstimatedHours { get; set; }
    public bool ClearEstimatedHours { get; set; } = false;
}

public class TaskStatsDto
{
    public int Total { get; set; }
    public int Backlog { get; set; }
    public int ToDo { get; set; }
    public int InProgress { get; set; }
    public int Review { get; set; }
    public int Done { get; set; }
    public int Overdue { get; set; }
}
