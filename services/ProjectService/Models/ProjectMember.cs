namespace ProjectService.Models;

public class ProjectMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public ProjectMemberRole Role { get; set; } = ProjectMemberRole.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public Project Project { get; set; } = null!;
}

public enum ProjectMemberRole { Owner, Manager, Member, Viewer }
