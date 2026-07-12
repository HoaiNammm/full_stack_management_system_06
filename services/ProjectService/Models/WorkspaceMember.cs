namespace ProjectService.Models;

public class WorkspaceMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkspaceId { get; set; }
    public Guid UserId { get; set; }
    public MemberRole Role { get; set; } = MemberRole.Member;
    public string? InviteMessage { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public Workspace Workspace { get; set; } = null!;
}

public enum MemberRole { Owner, Member, Viewer }
