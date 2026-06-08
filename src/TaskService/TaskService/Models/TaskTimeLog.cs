namespace TaskService.Models
{
    public class TaskTimeLog
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid LoggedBy { get; set; }
        public decimal Hours { get; set; }
        public string? Description { get; set; }
        public DateTime LoggedDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public TaskItem Task { get; set; } = null!;
    }
}
