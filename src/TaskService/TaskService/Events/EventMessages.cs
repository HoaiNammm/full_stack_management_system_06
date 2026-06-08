namespace TaskService.Events
{
    // task.column.changed — khi task được kéo sang column khác trên Kanban
    public class TaskColumnChangedEvent
    {
        public Guid TaskId { get; set; }
        public Guid ProjectId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public Guid OldColumnId { get; set; }
        public Guid NewColumnId { get; set; }
        public string NewColumnType { get; set; } = string.Empty; // backlog / active / done / custom
        public Guid? AssignedTo { get; set; }
        public Guid ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
    }

    // task.assigned
    public class TaskAssignedEvent
    {
        public Guid TaskId { get; set; }
        public Guid ProjectId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public Guid? PreviousAssignee { get; set; }
        public Guid? NewAssignee { get; set; }
        public Guid AssignedBy { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}
