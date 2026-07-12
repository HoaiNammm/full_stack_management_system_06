using Microsoft.EntityFrameworkCore;
using ProjectService.Data;
using ProjectService.DTOs.Members;
using ProjectService.DTOs.Workspaces;
using ProjectService.Events;
using ProjectService.HttpClients;
using ProjectService.Models;

namespace ProjectService.Services;

public class WorkspaceServiceImpl : IWorkspaceService
{
    private readonly ProjectDbContext _db;
    private readonly RabbitMqEventPublisher _publisher;
    private readonly NotifyServiceClient _notifyClient;

    public WorkspaceServiceImpl(ProjectDbContext db, RabbitMqEventPublisher publisher, NotifyServiceClient notifyClient)
    {
        _db = db;
        _publisher = publisher;
        _notifyClient = notifyClient;
    }

    public async Task<List<WorkspaceDto>> GetAllByUserAsync(Guid userId)
    {
        var workspaces = await _db.Workspaces
            .Include(w => w.Members)
            .Include(w => w.Projects)
            .Where(w => w.OwnerId == userId || w.Members.Any(m => m.UserId == userId))
            .ToListAsync();

        return workspaces.Select(MapToDto).ToList();
    }

    public async Task<WorkspaceDto?> GetByIdAsync(Guid id, Guid userId)
    {
        var workspace = await _db.Workspaces
            .Include(w => w.Members)
            .Include(w => w.Projects)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workspace is null) return null;
        if (!CanAccess(workspace, userId)) throw new UnauthorizedAccessException("Access denied.");

        return MapToDto(workspace);
    }

    public async Task<WorkspaceDto> CreateAsync(CreateWorkspaceRequest request, Guid ownerId)
    {
        var slug = GenerateSlug(request.Name);
        if (await _db.Workspaces.AnyAsync(w => w.Slug == slug))
            slug = $"{slug}-{Guid.NewGuid().ToString()[..8]}";

        var workspace = new Workspace
        {
            Name = request.Name,
            Slug = slug,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            OwnerId = ownerId
        };

        _db.Workspaces.Add(workspace);

        var ownerMember = new WorkspaceMember { WorkspaceId = workspace.Id, UserId = ownerId, Role = MemberRole.Owner };
        _db.WorkspaceMembers.Add(ownerMember);

        await _db.SaveChangesAsync();

        await _publisher.PublishAsync("workspace.created",
            new WorkspaceCreatedEvent(workspace.Id, workspace.Name, ownerId, DateTime.UtcNow));

        return MapToDto(workspace);
    }

    public async Task<WorkspaceDto> UpdateAsync(Guid id, UpdateWorkspaceRequest request, Guid userId)
    {
        var workspace = await _db.Workspaces.Include(w => w.Members).Include(w => w.Projects)
            .FirstOrDefaultAsync(w => w.Id == id)
            ?? throw new KeyNotFoundException("Workspace not found.");

        if (!IsOwner(workspace, userId)) throw new UnauthorizedAccessException("Only owner can update workspace.");

        if (request.Name is not null) workspace.Name = request.Name;
        if (request.Description is not null) workspace.Description = request.Description;
        if (request.ImageUrl is not null) workspace.ImageUrl = request.ImageUrl;
        workspace.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToDto(workspace);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var workspace = await _db.Workspaces.FindAsync(id)
            ?? throw new KeyNotFoundException("Workspace not found.");

        if (workspace.OwnerId != userId) throw new UnauthorizedAccessException("Only owner can delete workspace.");

        _db.Workspaces.Remove(workspace);
        await _db.SaveChangesAsync();
    }

    public async Task<List<WorkspaceMemberDto>> GetMembersAsync(Guid workspaceId, Guid requesterId)
    {
        var workspace = await _db.Workspaces.Include(w => w.Members).FirstOrDefaultAsync(w => w.Id == workspaceId)
            ?? throw new KeyNotFoundException("Workspace not found.");

        if (!CanAccess(workspace, requesterId)) throw new UnauthorizedAccessException("Access denied.");

        var members = workspace.Members.ToList();
        var users = await _notifyClient.GetUsersAsync(members.Select(m => m.UserId));
        var userMap = users.ToDictionary(u => u.Id);

        return members.Select(m => new WorkspaceMemberDto
        {
            Id = m.Id,
            WorkspaceId = m.WorkspaceId,
            UserId = m.UserId,
            Role = m.Role.ToString(),
            InviteMessage = m.InviteMessage,
            JoinedAt = m.JoinedAt,
            User = userMap.TryGetValue(m.UserId, out var u) ? u : null
        }).ToList();
    }

    public async Task<WorkspaceMemberDto> AddMemberAsync(Guid workspaceId, AddWorkspaceMemberRequest request, Guid requesterId)
    {
        var workspace = await _db.Workspaces.Include(w => w.Members).FirstOrDefaultAsync(w => w.Id == workspaceId)
            ?? throw new KeyNotFoundException("Workspace not found.");

        if (!IsOwner(workspace, requesterId)) throw new UnauthorizedAccessException("Only owner can add members.");

        // Resolve target user — accept UserId or Email
        Guid targetUserId;
        UserInfo? resolvedUser;

        if (request.UserId.HasValue && request.UserId.Value != Guid.Empty)
        {
            targetUserId = request.UserId.Value;
            resolvedUser = await _notifyClient.GetUserAsync(targetUserId)
                ?? throw new KeyNotFoundException("User not found.");
        }
        else if (!string.IsNullOrWhiteSpace(request.Email))
        {
            resolvedUser = await _notifyClient.GetUserByEmailAsync(request.Email)
                ?? throw new KeyNotFoundException($"No registered user with email '{request.Email}'.");
            targetUserId = resolvedUser.Id;
        }
        else
        {
            throw new ArgumentException("Provide either UserId or Email.");
        }

        if (workspace.Members.Any(m => m.UserId == targetUserId))
            throw new InvalidOperationException("User is already a member.");

        var role = Enum.Parse<MemberRole>(request.Role, ignoreCase: true);
        var member = new WorkspaceMember
        {
            WorkspaceId = workspaceId,
            UserId = targetUserId,
            Role = role,
            InviteMessage = request.InviteMessage
        };

        _db.WorkspaceMembers.Add(member);
        await _db.SaveChangesAsync();

        await _publisher.PublishAsync("workspace.member.added",
            new WorkspaceMemberAddedEvent(workspaceId, targetUserId, role.ToString(), DateTime.UtcNow));

        return new WorkspaceMemberDto
        {
            Id = member.Id,
            WorkspaceId = member.WorkspaceId,
            UserId = member.UserId,
            Role = member.Role.ToString(),
            InviteMessage = member.InviteMessage,
            JoinedAt = member.JoinedAt,
            User = resolvedUser
        };
    }

    public async Task<WorkspaceMemberDto> UpdateMemberRoleAsync(Guid workspaceId, Guid memberId, UpdateMemberRoleRequest request, Guid requesterId)
    {
        var workspace = await _db.Workspaces.Include(w => w.Members).FirstOrDefaultAsync(w => w.Id == workspaceId)
            ?? throw new KeyNotFoundException("Workspace not found.");

        if (!IsOwner(workspace, requesterId)) throw new UnauthorizedAccessException("Only owner can update roles.");

        var member = workspace.Members.FirstOrDefault(m => m.Id == memberId)
            ?? throw new KeyNotFoundException("Member not found.");

        member.Role = Enum.Parse<MemberRole>(request.Role, ignoreCase: true);
        await _db.SaveChangesAsync();

        return new WorkspaceMemberDto { Id = member.Id, WorkspaceId = member.WorkspaceId, UserId = member.UserId, Role = member.Role.ToString(), JoinedAt = member.JoinedAt };
    }

    public async Task RemoveMemberAsync(Guid workspaceId, Guid memberId, Guid requesterId)
    {
        var workspace = await _db.Workspaces.Include(w => w.Members).FirstOrDefaultAsync(w => w.Id == workspaceId)
            ?? throw new KeyNotFoundException("Workspace not found.");

        if (!IsOwner(workspace, requesterId)) throw new UnauthorizedAccessException("Only owner can remove members.");

        var member = workspace.Members.FirstOrDefault(m => m.Id == memberId)
            ?? throw new KeyNotFoundException("Member not found.");

        _db.WorkspaceMembers.Remove(member);
        await _db.SaveChangesAsync();

        await _publisher.PublishAsync("workspace.member.removed",
            new WorkspaceMemberRemovedEvent(workspaceId, member.UserId, DateTime.UtcNow));
    }

    private static bool CanAccess(Workspace w, Guid userId) =>
        w.OwnerId == userId || w.Members.Any(m => m.UserId == userId);

    private static bool IsOwner(Workspace w, Guid userId) =>
        w.OwnerId == userId || w.Members.Any(m => m.UserId == userId && m.Role == MemberRole.Owner);

    private static string GenerateSlug(string name) =>
        name.ToLower().Replace(" ", "-").Replace("_", "-");

    private static WorkspaceDto MapToDto(Workspace w) => new()
    {
        Id = w.Id,
        Name = w.Name,
        Slug = w.Slug,
        Description = w.Description,
        ImageUrl = w.ImageUrl,
        OwnerId = w.OwnerId,
        MemberCount = w.Members.Count,
        ProjectCount = w.Projects.Count,
        CreatedAt = w.CreatedAt,
        UpdatedAt = w.UpdatedAt
    };
}
