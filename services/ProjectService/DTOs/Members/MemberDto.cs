using System.ComponentModel.DataAnnotations;

namespace ProjectService.DTOs.Members;

public class WorkspaceMemberDto
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? InviteMessage { get; set; }
    public DateTime JoinedAt { get; set; }
    public UserInfo? User { get; set; }
}

public class ProjectMemberDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public UserInfo? User { get; set; }
}

public class UserInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}

public class AddWorkspaceMemberRequest
{
    // Provide either UserId OR Email — at least one is required
    public Guid? UserId { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    public string Role { get; set; } = "Member";

    [MaxLength(300)]
    public string? InviteMessage { get; set; }
}

public class AddProjectMemberRequest
{
    [Required]
    public Guid UserId { get; set; }
    public string Role { get; set; } = "Member";
}

public class UpdateMemberRoleRequest
{
    [Required]
    public string Role { get; set; } = string.Empty;
}
