namespace TaskService.Models;

public class SubTask
{
    public Guid      Id             { get; set; } = Guid.NewGuid();
    public Guid      TaskId         { get; set; }
    public string    Title          { get; set; } = string.Empty;
    public string?   Description    { get; set; }
    public Guid?     AssignedTo     { get; set; }
    public int       Status         { get; set; } = 0;
    public bool      IsCompleted    { get; set; } = false;
    public decimal?  EstimatedHours { get; set; }
    public DateTime  CreatedAt      { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt      { get; set; }
    public DateTime? DeletedAt      { get; set; }

    public TaskItem? Task { get; set; }
}
