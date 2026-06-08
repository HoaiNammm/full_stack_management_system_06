namespace TaskService.Models
{
    public class TaskItem
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? SprintId { get; set; }
        public Guid ColumnId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? AssignedTo { get; set; }
        public int Priority { get; set; } // Low=0, Medium=1, High=2
        public decimal? EstimatedHours { get; set; }
        public DateTime? Deadline { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public KanbanColumn Column { get; set; } = null!;
        public ICollection<SubTask> SubTasks { get; set; } = new List<SubTask>();
        public ICollection<TaskTimeLog> TimeLogs { get; set; } = new List<TaskTimeLog>();
        public ICollection<TaskAssignmentHistory> AssignmentHistory { get; set; } = new List<TaskAssignmentHistory>();
    }
}
