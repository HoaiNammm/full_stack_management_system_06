namespace ProjectService.Models
{
    public class Member
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }
        public int Role { get; set; } // Owner=0, Manager=1, Member=2, Viewer=3
        public DateTime JoinedAt { get; set; }

        public Project Project { get; set; }
    }
}