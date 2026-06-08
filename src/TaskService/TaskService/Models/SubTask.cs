namespace TaskService.Models
{
    public class SubTask
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? AssignedTo { get; set; }
        public int Status { get; set; } // Backlog=0, ToDo=1, InProgress=2, Review=3, Done=4
        public decimal? EstimatedHours { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public TaskItem Task { get; set; } = null!;
    }
}
