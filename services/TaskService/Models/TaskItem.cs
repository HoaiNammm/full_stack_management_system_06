namespace TaskService.Models;

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Guid? SprintId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public KanbanStatus Status { get; set; } = KanbanStatus.Backlog;
    public TaskItemType Type { get; set; } = TaskItemType.Task;
    public TaskItemPriority Priority { get; set; } = TaskItemPriority.Medium;
    public string? Labels { get; set; }
    public Guid? AssigneeId { get; set; }
    public DateTime? Deadline { get; set; }
    public decimal? EstimatedHours { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<SubTask> SubTasks { get; set; } = new List<SubTask>();
    public ICollection<TimeLog> TimeLogs { get; set; } = new List<TimeLog>();
}

public enum KanbanStatus { Backlog, ToDo, InProgress, Review, Done }
public enum TaskItemType { Feature, Bug, Task, Improvement, Other }
public enum TaskItemPriority { Low, Medium, High }
