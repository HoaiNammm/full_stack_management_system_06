using System.ComponentModel.DataAnnotations;

namespace TaskService.DTOs.SubTasks;

public class SubTaskDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public Guid? AssigneeId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSubTaskRequest
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public Guid? AssigneeId { get; set; }
}

public class UpdateSubTaskRequest
{
    [MaxLength(200)]
    public string? Title { get; set; }

    public bool? IsCompleted { get; set; }
    public Guid? AssigneeId { get; set; }
}
