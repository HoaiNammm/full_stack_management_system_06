namespace ProjectService.Models
{
    public class Sprint
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Goal { get; set; }  // Sprint goal / objective
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }  // Auto: StartDate + 14 days
        public int Status { get; set; } = 0; // Draft=0, Active=1, Completed=2
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Project Project { get; set; } = null!;
    }
}