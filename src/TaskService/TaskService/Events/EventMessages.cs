namespace TaskService.Events;

public class TaskColumnChangedEvent
{
    public Guid     TaskId        { get; set; }
    public Guid     ProjectId     { get; set; }
    public string   TaskTitle     { get; set; } = string.Empty;
    public string   NewColumnType { get; set; } = string.Empty;
    public Guid     OldColumnId   { get; set; }
    public Guid     NewColumnId   { get; set; }
    public Guid?    AssignedTo    { get; set; }
    public Guid     ChangedBy     { get; set; }
    public DateTime ChangedAt     { get; set; }
}

public class TaskAssignedEvent
{
    public Guid     TaskId      { get; set; }
    public Guid     ProjectId   { get; set; }
    public string   TaskTitle   { get; set; } = string.Empty;
    public Guid     AssignedTo  { get; set; }
    public Guid     AssignedBy  { get; set; }
    public DateTime AssignedAt  { get; set; }
}
