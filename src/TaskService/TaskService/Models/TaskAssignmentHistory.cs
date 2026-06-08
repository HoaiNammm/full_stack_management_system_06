namespace TaskService.Models
{
    public class TaskAssignmentHistory
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid? PreviousAssignee { get; set; }
        public Guid? NewAssignee { get; set; }
        public Guid ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }

        public TaskItem Task { get; set; } = null!;
    }
}
