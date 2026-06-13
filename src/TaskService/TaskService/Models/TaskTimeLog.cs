namespace TaskService.Models;

public class TaskTimeLog
{
    public Guid      Id          { get; set; } = Guid.NewGuid();
    public Guid      TaskId      { get; set; }
    public Guid      UserId      { get; set; }
    public Guid      LoggedBy    { get; set; }
    public decimal   Hours       { get; set; }
    public DateTime  LoggedAt    { get; set; } = DateTime.UtcNow;
    public DateTime  LoggedDate  { get; set; } = DateTime.UtcNow;
    public string?   Description { get; set; }
    public DateTime  CreatedAt   { get; set; } = DateTime.UtcNow;

    public TaskItem? Task { get; set; }
}
