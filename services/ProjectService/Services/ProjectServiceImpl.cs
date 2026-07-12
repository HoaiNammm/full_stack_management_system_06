using Microsoft.EntityFrameworkCore;
using ProjectService.Data;
using ProjectService.DTOs.Members;
using ProjectService.DTOs.Projects;
using ProjectService.DTOs.Sprints;
using ProjectService.Events;
using ProjectService.HttpClients;
using ProjectService.Models;

namespace ProjectService.Services;

public class ProjectServiceImpl : IProjectService
{
    private readonly ProjectDbContext _db;
    private readonly RabbitMqEventPublisher _publisher;
    private readonly NotifyServiceClient _notifyClient;

    public ProjectServiceImpl(ProjectDbContext db, RabbitMqEventPublisher publisher, NotifyServiceClient notifyClient)
    {
        _db = db;
        _publisher = publisher;
        _notifyClient = notifyClient;
    }

    // ─── Projects ────────────────────────────────────────────────────────────

    public async Task<List<ProjectDto>> GetByWorkspaceAsync(Guid workspaceId, Guid userId)
    {
        await EnsureWorkspaceAccessAsync(workspaceId, userId);

        var projects = await _db.Projects
            .Include(p => p.Members)
            .Include(p => p.Sprints)
            .Where(p => p.WorkspaceId == workspaceId)
            .ToListAsync();

        return projects.Select(MapToDto).ToList();
    }

    public async Task<ProjectDto?> GetByIdAsync(Guid id, Guid userId)
    {
        var project = await _db.Projects
            .Include(p => p.Members)
            .Include(p => p.Sprints)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (project is null) return null;
        await EnsureWorkspaceAccessAsync(project.WorkspaceId, userId);
        return MapToDto(project);
    }

    public async Task<ProjectDto> CreateAsync(Guid workspaceId, CreateProjectRequest request, Guid userId)
    {
        await EnsureWorkspaceAccessAsync(workspaceId, userId);

        var project = new Project
        {
            WorkspaceId = workspaceId,
            Name = request.Name,
            Description = request.Description,
            Color = request.Color,
            Priority = Enum.Parse<ProjectPriority>(request.Priority, ignoreCase: true),
            Status = Enum.Parse<ProjectStatus>(request.Status, ignoreCase: true),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TeamLeadId = request.TeamLeadId
        };

        _db.Projects.Add(project);

        var ownerMember = new ProjectMember { ProjectId = project.Id, UserId = userId, Role = ProjectMemberRole.Owner };
        _db.ProjectMembers.Add(ownerMember);

        await _db.SaveChangesAsync();

        await _publisher.PublishAsync("project.created",
            new ProjectCreatedEvent(project.Id, workspaceId, project.Name, userId, DateTime.UtcNow));

        return MapToDto(project);
    }

    public async Task<ProjectDto> UpdateAsync(Guid id, UpdateProjectRequest request, Guid userId)
    {
        var project = await _db.Projects.Include(p => p.Members).Include(p => p.Sprints)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Project not found.");

        EnsureManager(project, userId);

        if (request.Name is not null) project.Name = request.Name;
        if (request.Description is not null) project.Description = request.Description;
        if (request.Color is not null) project.Color = request.Color;
        if (request.Priority is not null) project.Priority = Enum.Parse<ProjectPriority>(request.Priority, ignoreCase: true);
        if (request.Status is not null) project.Status = Enum.Parse<ProjectStatus>(request.Status, ignoreCase: true);
        if (request.StartDate.HasValue) project.StartDate = request.StartDate;
        if (request.EndDate.HasValue) project.EndDate = request.EndDate;
        if (request.TeamLeadId.HasValue) project.TeamLeadId = request.TeamLeadId;
        if (request.Progress.HasValue) project.Progress = Math.Clamp(request.Progress.Value, 0, 100);
        project.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _publisher.PublishAsync("project.updated",
            new ProjectUpdatedEvent(project.Id, project.Name, project.Status.ToString(), DateTime.UtcNow));

        return MapToDto(project);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var project = await _db.Projects.Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException("Project not found.");

        EnsureOwner(project, userId);
        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();
    }

    // ─── Members ─────────────────────────────────────────────────────────────

    public async Task<List<ProjectMemberDto>> GetMembersAsync(Guid projectId, Guid userId)
    {
        var project = await GetProjectOrThrowAsync(projectId);
        await EnsureWorkspaceAccessAsync(project.WorkspaceId, userId);

        var users = await _notifyClient.GetUsersAsync(project.Members.Select(m => m.UserId));
        var userMap = users.ToDictionary(u => u.Id);

        return project.Members.Select(m => new ProjectMemberDto
        {
            Id = m.Id,
            ProjectId = m.ProjectId,
            UserId = m.UserId,
            Role = m.Role.ToString(),
            JoinedAt = m.JoinedAt,
            User = userMap.TryGetValue(m.UserId, out var u) ? u : null
        }).ToList();
    }

    public async Task<ProjectMemberDto> AddMemberAsync(Guid projectId, AddProjectMemberRequest request, Guid userId)
    {
        var project = await GetProjectOrThrowAsync(projectId);
        EnsureManager(project, userId);

        if (project.Members.Any(m => m.UserId == request.UserId))
            throw new InvalidOperationException("User is already a project member.");

        var role = Enum.Parse<ProjectMemberRole>(request.Role, ignoreCase: true);
        var member = new ProjectMember { ProjectId = projectId, UserId = request.UserId, Role = role };

        _db.ProjectMembers.Add(member);
        await _db.SaveChangesAsync();

        await _publisher.PublishAsync("project.member.added",
            new ProjectMemberAddedEvent(projectId, project.WorkspaceId, request.UserId, role.ToString(), DateTime.UtcNow));

        var user = await _notifyClient.GetUserAsync(request.UserId);
        return new ProjectMemberDto { Id = member.Id, ProjectId = member.ProjectId, UserId = member.UserId, Role = member.Role.ToString(), JoinedAt = member.JoinedAt, User = user };
    }

    public async Task<ProjectMemberDto> UpdateMemberRoleAsync(Guid projectId, Guid memberId, UpdateMemberRoleRequest request, Guid userId)
    {
        var project = await GetProjectOrThrowAsync(projectId);
        EnsureManager(project, userId);

        var member = project.Members.FirstOrDefault(m => m.Id == memberId)
            ?? throw new KeyNotFoundException("Member not found.");

        member.Role = Enum.Parse<ProjectMemberRole>(request.Role, ignoreCase: true);
        await _db.SaveChangesAsync();

        return new ProjectMemberDto { Id = member.Id, ProjectId = member.ProjectId, UserId = member.UserId, Role = member.Role.ToString(), JoinedAt = member.JoinedAt };
    }

    public async Task RemoveMemberAsync(Guid projectId, Guid memberId, Guid userId)
    {
        var project = await GetProjectOrThrowAsync(projectId);
        EnsureManager(project, userId);

        var member = project.Members.FirstOrDefault(m => m.Id == memberId)
            ?? throw new KeyNotFoundException("Member not found.");

        _db.ProjectMembers.Remove(member);
        await _db.SaveChangesAsync();

        await _publisher.PublishAsync("project.member.removed",
            new ProjectMemberRemovedEvent(projectId, member.UserId, DateTime.UtcNow));
    }

    public async Task<List<Guid>> GetMemberIdsAsync(Guid projectId) =>
        await _db.ProjectMembers.Where(m => m.ProjectId == projectId).Select(m => m.UserId).ToListAsync();

    // ─── Sprints ─────────────────────────────────────────────────────────────

    public async Task<List<SprintDto>> GetSprintsAsync(Guid projectId, Guid userId)
    {
        var project = await GetProjectOrThrowAsync(projectId);
        await EnsureWorkspaceAccessAsync(project.WorkspaceId, userId);

        var sprints = await _db.Sprints.Where(s => s.ProjectId == projectId)
            .OrderBy(s => s.StartDate).ToListAsync();
        return sprints.Select(MapSprintDto).ToList();
    }

    public async Task<SprintDto> CreateSprintAsync(Guid projectId, CreateSprintRequest request, Guid userId)
    {
        var project = await GetProjectOrThrowAsync(projectId);
        EnsureManager(project, userId);

        if (request.StartDate.Date < DateTime.UtcNow.Date)
            throw new InvalidOperationException("Sprint start date cannot be in the past.");

        var sprint = Sprint.CreateTwoWeek(projectId, request.Name, request.Goal, request.StartDate);
        _db.Sprints.Add(sprint);
        await _db.SaveChangesAsync();
        return MapSprintDto(sprint);
    }

    public async Task<SprintDto> UpdateSprintAsync(Guid projectId, Guid sprintId, UpdateSprintRequest request, Guid userId)
    {
        var project = await GetProjectOrThrowAsync(projectId);
        EnsureManager(project, userId);

        var sprint = await _db.Sprints.FirstOrDefaultAsync(s => s.Id == sprintId && s.ProjectId == projectId)
            ?? throw new KeyNotFoundException("Sprint not found.");
        if (sprint.Status == SprintStatus.Active)
            throw new InvalidOperationException("Cannot edit an active sprint.");

        if (request.Name is not null) sprint.Name = request.Name;
        if (request.Goal is not null) sprint.Goal = request.Goal;
        if (request.StartDate.HasValue) sprint.StartDate = request.StartDate.Value;
        if (request.EndDate.HasValue) sprint.EndDate = request.EndDate.Value;
        else if (request.StartDate.HasValue) sprint.EndDate = request.StartDate.Value.AddDays(14);
        sprint.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapSprintDto(sprint);
    }

    public async Task<SprintDto> StartSprintAsync(Guid projectId, Guid sprintId, Guid userId)
    {
        var project = await GetProjectOrThrowAsync(projectId);
        EnsureManager(project, userId);

        if (await _db.Sprints.AnyAsync(s => s.ProjectId == projectId && s.Status == SprintStatus.Active))
            throw new InvalidOperationException("A sprint is already active. Complete it first.");

        var sprint = await _db.Sprints.FirstOrDefaultAsync(s => s.Id == sprintId && s.ProjectId == projectId)
            ?? throw new KeyNotFoundException("Sprint not found.");

        sprint.Status = SprintStatus.Active;
        sprint.StartDate = DateTime.UtcNow;
        sprint.EndDate = DateTime.UtcNow.AddDays(14);
        sprint.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _publisher.PublishAsync("sprint.started",
            new SprintStartedEvent(sprint.Id, projectId, sprint.Name, sprint.Goal, sprint.StartDate, sprint.EndDate, DateTime.UtcNow));

        return MapSprintDto(sprint);
    }

    public async Task<SprintDto> CompleteSprintAsync(Guid projectId, Guid sprintId, Guid userId)
    {
        var project = await GetProjectOrThrowAsync(projectId);
        EnsureManager(project, userId);

        var sprint = await _db.Sprints.FirstOrDefaultAsync(s => s.Id == sprintId && s.ProjectId == projectId)
            ?? throw new KeyNotFoundException("Sprint not found.");
        if (sprint.Status != SprintStatus.Active)
            throw new InvalidOperationException("Only active sprints can be completed.");

        sprint.Status = SprintStatus.Completed;
        sprint.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _publisher.PublishAsync("sprint.completed",
            new SprintCompletedEvent(sprint.Id, projectId, sprint.Name, DateTime.UtcNow));

        return MapSprintDto(sprint);
    }

    public async Task DeleteSprintAsync(Guid projectId, Guid sprintId, Guid userId)
    {
        var project = await GetProjectOrThrowAsync(projectId);
        EnsureManager(project, userId);

        var sprint = await _db.Sprints.FirstOrDefaultAsync(s => s.Id == sprintId && s.ProjectId == projectId)
            ?? throw new KeyNotFoundException("Sprint not found.");
        if (sprint.Status == SprintStatus.Active)
            throw new InvalidOperationException("Cannot delete an active sprint.");
        _db.Sprints.Remove(sprint);
        await _db.SaveChangesAsync();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private async Task<Project> GetProjectOrThrowAsync(Guid projectId) =>
        await _db.Projects.Include(p => p.Members).Include(p => p.Sprints)
            .FirstOrDefaultAsync(p => p.Id == projectId)
        ?? throw new KeyNotFoundException("Project not found.");

    private async Task EnsureWorkspaceAccessAsync(Guid workspaceId, Guid userId)
    {
        var isMember = await _db.WorkspaceMembers.AnyAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId);
        var isOwner = await _db.Workspaces.AnyAsync(w => w.Id == workspaceId && w.OwnerId == userId);
        if (!isMember && !isOwner) throw new UnauthorizedAccessException("Access denied.");
    }

    private static void EnsureManager(Project project, Guid userId)
    {
        var isManager = project.Members.Any(m => m.UserId == userId &&
            (m.Role == ProjectMemberRole.Owner || m.Role == ProjectMemberRole.Manager));
        if (!isManager) throw new UnauthorizedAccessException("Only Owner or Manager can perform this action.");
    }

    private static void EnsureOwner(Project project, Guid userId)
    {
        var isOwner = project.Members.Any(m => m.UserId == userId && m.Role == ProjectMemberRole.Owner);
        if (!isOwner) throw new UnauthorizedAccessException("Only Owner can delete the project.");
    }

    private static ProjectDto MapToDto(Project p) => new()
    {
        Id = p.Id,
        WorkspaceId = p.WorkspaceId,
        Name = p.Name,
        Description = p.Description,
        Color = p.Color,
        Priority = p.Priority.ToString(),
        Status = p.Status.ToString(),
        StartDate = p.StartDate,
        EndDate = p.EndDate,
        TeamLeadId = p.TeamLeadId,
        Progress = p.Progress,
        MemberCount = p.Members.Count,
        SprintCount = p.Sprints.Count,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };

    private static SprintDto MapSprintDto(Sprint s) => new()
    {
        Id = s.Id,
        ProjectId = s.ProjectId,
        Name = s.Name,
        Goal = s.Goal,
        StartDate = s.StartDate,
        EndDate = s.EndDate,
        Status = s.Status.ToString(),
        CreatedAt = s.CreatedAt
    };
}
