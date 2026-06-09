namespace TaskService.Models;

public class TaskItem
{
    public Guid      Id              { get; set; } = Guid.NewGuid();
    public Guid      ProjectId       { get; set; }
    public Guid      ColumnId        { get; set; }
    public string    Title           { get; set; } = string.Empty;
    public string?   Description     { get; set; }
    public int       Priority        { get; set; } = 2; // 1=Low 2=Medium 3=High
    public Guid?     AssignedTo      { get; set; }
    public Guid?     ReporterId      { get; set; }
    public DateTime? DueDate         { get; set; }
    public decimal?  EstimatedHours  { get; set; }
    public Guid?     SprintId        { get; set; }
    public DateTime  CreatedAt       { get; set; } = DateTime.UtcNow;
    public Guid      CreatedBy       { get; set; }
    public DateTime? DeletedAt       { get; set; }

    public KanbanColumn? Column { get; set; }
    public ICollection<SubTask>      SubTasks     { get; set; } = new List<SubTask>();
    public ICollection<TaskTimeLog>  TimeLogs     { get; set; } = new List<TaskTimeLog>();
}
