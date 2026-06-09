namespace TaskService.Models;

public class KanbanColumn
{
    public Guid   Id        { get; set; } = Guid.NewGuid();
    public Guid   ProjectId { get; set; }
    public string Name      { get; set; } = string.Empty;
    public int    Position  { get; set; }
    // backlog | active | done | custom
    public string Type      { get; set; } = "custom";

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
