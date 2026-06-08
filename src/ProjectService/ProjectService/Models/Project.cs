namespace ProjectService.Models
{
    public class Project
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Status { get; set; } = 0; // Draft=0, Active=1, Completed=2
        public string? Color { get; set; }  // Hex color, e.g. "#4F46E5"
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Member> Members { get; set; } = new List<Member>();
        public ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();
        public ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
    }
}