namespace TaskService.Models
{
    public class KanbanBoard
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public ICollection<KanbanColumn> Columns { get; set; } = new List<KanbanColumn>();
    }
}
