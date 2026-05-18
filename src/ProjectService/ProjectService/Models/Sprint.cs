namespace ProjectService.Models
{
    public class Sprint
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Project Project { get; set; }
    }
}