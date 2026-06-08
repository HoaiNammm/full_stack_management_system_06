namespace ProjectService.Models
{
    public class Milestone
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime TargetDate { get; set; }
        public int Status { get; set; } = 0; // NotStarted=0, InProgress=1, Completed=2
        public DateTime CreatedAt { get; set; }

        public Project Project { get; set; }
    }
}
