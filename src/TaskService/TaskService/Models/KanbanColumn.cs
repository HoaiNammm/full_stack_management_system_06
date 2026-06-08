namespace TaskService.Models
{
    public class KanbanColumn
    {
        public Guid Id { get; set; }
        public Guid BoardId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "custom"; // backlog / active / done / custom
        public int Position { get; set; }

        public KanbanBoard Board { get; set; } = null!;
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
