using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using TaskService.DTOs.Tasks;
using TaskService.Events;
using TaskService.HttpClients;
using TaskService.Models;

namespace TaskService.Services;

public class TaskServiceImpl : ITaskService
{
    private readonly TaskDbContext _db;
    private readonly RabbitMqEventPublisher _publisher;
    private readonly WorkspaceServiceClient _projectClient;
    private readonly UserServiceClient _userClient;

    public TaskServiceImpl(TaskDbContext db, RabbitMqEventPublisher publisher,
        WorkspaceServiceClient projectClient, UserServiceClient userClient)
    {
        _db = db;
        _publisher = publisher;
        _projectClient = projectClient;
        _userClient = userClient;
    }

    public async Task<List<TaskDto>> GetByProjectAsync(Guid projectId, Guid userId, string? status, string? priority)
    {
        await EnsureProjectAccessAsync(projectId, userId);
        var query = _db.Tasks.Include(t => t.SubTasks).Include(t => t.TimeLogs)
            .Where(t => t.ProjectId == projectId);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<KanbanStatus>(status, true, out var s))
            query = query.Where(t => t.Status == s);
        if (!string.IsNullOrEmpty(priority) && Enum.TryParse<TaskItemPriority>(priority, true, out var p))
            query = query.Where(t => t.Priority == p);

        var tasks = await query.OrderBy(t => t.Status).ThenByDescending(t => t.CreatedAt).ToListAsync();
        var assigneeIds = tasks.Where(t => t.AssigneeId.HasValue).Select(t => t.AssigneeId!.Value).Distinct();
        var users = await _userClient.GetUsersAsync(assigneeIds);
        var userMap = users.ToDictionary(u => u.Id);
        return tasks.Select(t => MapToDto(t, userMap)).ToList();
    }

    public async Task<TaskDto?> GetByIdAsync(Guid id, Guid userId)
    {
        var task = await _db.Tasks.Include(t => t.SubTasks).Include(t => t.TimeLogs)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (task is null) return null;
        await EnsureProjectAccessAsync(task.ProjectId, userId);
        UserInfo? assignee = task.AssigneeId.HasValue ? await _userClient.GetUserAsync(task.AssigneeId.Value) : null;
        var map = assignee is null ? new() : new Dictionary<Guid, UserInfo> { [assignee.Id] = assignee };
        return MapToDto(task, map);
    }

    public async Task<TaskDto> CreateAsync(Guid projectId, CreateTaskRequest request, Guid userId)
    {
        await EnsureProjectAccessAsync(projectId, userId);
        var task = new TaskItem
        {
            ProjectId = projectId,
            SprintId = request.SprintId,
            Title = request.Title,
            Description = request.Description,
            Status = Enum.Parse<KanbanStatus>(request.Status, ignoreCase: true),
            Type = Enum.Parse<TaskItemType>(request.Type, ignoreCase: true),
            Priority = Enum.Parse<TaskItemPriority>(request.Priority, ignoreCase: true),
            Labels = request.Labels.Count > 0 ? JsonSerializer.Serialize(request.Labels) : null,
            AssigneeId = request.AssigneeId,
            Deadline = request.Deadline,
            EstimatedHours = request.EstimatedHours,
            CreatedById = userId
        };
        _db.Tasks.Add(task);
        _db.ActivityLogs.Add(new Models.ActivityLog { ProjectId = projectId, TaskId = task.Id, ActorId = userId, Action = "TaskCreated", EntityName = task.Title });
        await _db.SaveChangesAsync();

        await _publisher.PublishAsync("task.created", new TaskCreatedEvent(task.Id, projectId, task.Title, userId, DateTime.UtcNow));
        if (task.AssigneeId.HasValue && task.AssigneeId != userId)
            await _publisher.PublishAsync("task.assigned", new TaskAssignedEvent(task.Id, projectId, task.Title, task.AssigneeId.Value, userId, DateTime.UtcNow));

        return MapToDto(task, new());
    }

    public async Task<TaskDto> UpdateAsync(Guid id, UpdateTaskRequest request, Guid userId)
    {
        var task = await _db.Tasks.Include(t => t.SubTasks).Include(t => t.TimeLogs)
            .FirstOrDefaultAsync(t => t.Id == id) ?? throw new KeyNotFoundException("Task not found.");
        await EnsureProjectAccessAsync(task.ProjectId, userId);

        var oldStatus = task.Status;
        var oldAssignee = task.AssigneeId;

        if (request.Title is not null) task.Title = request.Title;
        if (request.Description is not null) task.Description = request.Description;
        if (request.Status is not null) task.Status = Enum.Parse<KanbanStatus>(request.Status, ignoreCase: true);
        if (request.Type is not null) task.Type = Enum.Parse<TaskItemType>(request.Type, ignoreCase: true);
        if (request.Priority is not null) task.Priority = Enum.Parse<TaskItemPriority>(request.Priority, ignoreCase: true);
        if (request.Labels is not null) task.Labels = request.Labels.Count > 0 ? JsonSerializer.Serialize(request.Labels) : null;
        if (request.AssigneeId.HasValue) task.AssigneeId = request.AssigneeId;
        else if (request.ClearAssignee) task.AssigneeId = null;
        if (request.Deadline.HasValue) task.Deadline = request.Deadline;
        else if (request.ClearDeadline) task.Deadline = null;
        if (request.SprintId.HasValue) task.SprintId = request.SprintId;
        else if (request.ClearSprint) task.SprintId = null;
        if (request.EstimatedHours.HasValue) task.EstimatedHours = request.EstimatedHours;
        else if (request.ClearEstimatedHours) task.EstimatedHours = null;
        task.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        if (task.Status != oldStatus)
        {
            _db.ActivityLogs.Add(new Models.ActivityLog { ProjectId = task.ProjectId, TaskId = task.Id, ActorId = userId, Action = "TaskStatusChanged", EntityName = task.Title, Description = $"{oldStatus} → {task.Status}" });
            await _db.SaveChangesAsync();
            await _publisher.PublishAsync("task.status.changed",
                new TaskStatusChangedEvent(task.Id, task.ProjectId, task.Title, oldStatus.ToString(), task.Status.ToString(), userId, DateTime.UtcNow));
        }

        if (task.AssigneeId.HasValue && task.AssigneeId != oldAssignee && task.AssigneeId != userId)
            await _publisher.PublishAsync("task.assigned",
                new TaskAssignedEvent(task.Id, task.ProjectId, task.Title, task.AssigneeId.Value, userId, DateTime.UtcNow));

        return MapToDto(task, new());
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var task = await _db.Tasks.FindAsync(id) ?? throw new KeyNotFoundException("Task not found.");
        await EnsureProjectAccessAsync(task.ProjectId, userId);
        _db.ActivityLogs.Add(new Models.ActivityLog { ProjectId = task.ProjectId, TaskId = task.Id, ActorId = userId, Action = "TaskDeleted", EntityName = task.Title });
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        await _publisher.PublishAsync("task.deleted", new TaskDeletedEvent(task.Id, task.ProjectId, DateTime.UtcNow));
    }

    public async Task<TaskStatsDto> GetStatsByProjectAsync(Guid projectId)
    {
        var tasks = await _db.Tasks.Where(t => t.ProjectId == projectId).ToListAsync();
        var now = DateTime.UtcNow;
        return new TaskStatsDto
        {
            Total = tasks.Count,
            Backlog = tasks.Count(t => t.Status == KanbanStatus.Backlog),
            ToDo = tasks.Count(t => t.Status == KanbanStatus.ToDo),
            InProgress = tasks.Count(t => t.Status == KanbanStatus.InProgress),
            Review = tasks.Count(t => t.Status == KanbanStatus.Review),
            Done = tasks.Count(t => t.Status == KanbanStatus.Done),
            Overdue = tasks.Count(t => t.Deadline.HasValue && t.Deadline < now && t.Status != KanbanStatus.Done)
        };
    }

    public async Task<List<TaskDto>> GetAssignedToMeAsync(Guid userId)
    {
        var tasks = await _db.Tasks
            .Include(t => t.SubTasks).Include(t => t.TimeLogs)
            .Where(t => t.AssigneeId == userId)
            .OrderBy(t => t.Deadline).ThenByDescending(t => t.UpdatedAt)
            .ToListAsync();
        UserInfo? me = null;
        try { me = await _userClient.GetUserAsync(userId); } catch { }
        var userMap = me is null ? new Dictionary<Guid, UserInfo>() : new Dictionary<Guid, UserInfo> { [me.Id] = me };
        return tasks.Select(t => MapToDto(t, userMap)).ToList();
    }

    private async Task EnsureProjectAccessAsync(Guid projectId, Guid userId)
    {
        var isMember = await _projectClient.IsProjectMemberAsync(projectId, userId);
        if (!isMember) throw new UnauthorizedAccessException("You are not a member of this project.");
    }

    private static TaskDto MapToDto(TaskItem t, Dictionary<Guid, UserInfo> userMap) => new()
    {
        Id = t.Id,
        ProjectId = t.ProjectId,
        SprintId = t.SprintId,
        Title = t.Title,
        Description = t.Description,
        Status = t.Status.ToString(),
        Type = t.Type.ToString(),
        Priority = t.Priority.ToString(),
        Labels = t.Labels is not null ? JsonSerializer.Deserialize<List<string>>(t.Labels) ?? new() : new(),
        AssigneeId = t.AssigneeId,
        Assignee = t.AssigneeId.HasValue && userMap.TryGetValue(t.AssigneeId.Value, out var u)
            ? new AssigneeInfo { Id = u.Id, Name = u.Name, Email = u.Email, AvatarUrl = u.AvatarUrl }
            : null,
        Deadline = t.Deadline,
        EstimatedHours = t.EstimatedHours,
        CreatedById = t.CreatedById,
        SubTaskCount = t.SubTasks.Count,
        CompletedSubTaskCount = t.SubTasks.Count(s => s.IsCompleted),
        TotalHoursLogged = t.TimeLogs.Sum(l => l.HoursLogged),
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt
    };
}
