using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using TaskService.DTOs.SubTasks;
using TaskService.HttpClients;
using TaskService.Models;

namespace TaskService.Services;

public class SubTaskServiceImpl : ISubTaskService
{
    private readonly TaskDbContext _db;
    private readonly WorkspaceServiceClient _projectClient;

    public SubTaskServiceImpl(TaskDbContext db, WorkspaceServiceClient projectClient)
    {
        _db = db;
        _projectClient = projectClient;
    }

    public async Task<List<SubTaskDto>> GetByTaskAsync(Guid taskId, Guid userId)
    {
        var task = await _db.Tasks.FindAsync(taskId) ?? throw new KeyNotFoundException("Task not found.");
        await EnsureProjectAccessAsync(task.ProjectId, userId);
        var subtasks = await _db.SubTasks.Where(s => s.TaskId == taskId).OrderBy(s => s.CreatedAt).ToListAsync();
        return subtasks.Select(MapToDto).ToList();
    }

    public async Task<SubTaskDto> CreateAsync(Guid taskId, CreateSubTaskRequest request, Guid userId)
    {
        var task = await _db.Tasks.FindAsync(taskId) ?? throw new KeyNotFoundException("Task not found.");
        await EnsureProjectAccessAsync(task.ProjectId, userId);
        var subtask = new SubTask { TaskId = taskId, Title = request.Title, AssigneeId = request.AssigneeId };
        _db.SubTasks.Add(subtask);
        await _db.SaveChangesAsync();
        return MapToDto(subtask);
    }

    public async Task<SubTaskDto> UpdateAsync(Guid id, UpdateSubTaskRequest request, Guid userId)
    {
        var subtask = await _db.SubTasks.Include(s => s.Task).FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new KeyNotFoundException("SubTask not found.");
        await EnsureProjectAccessAsync(subtask.Task.ProjectId, userId);
        if (request.Title is not null) subtask.Title = request.Title;
        if (request.IsCompleted.HasValue) subtask.IsCompleted = request.IsCompleted.Value;
        if (request.AssigneeId.HasValue) subtask.AssigneeId = request.AssigneeId;
        subtask.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToDto(subtask);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var subtask = await _db.SubTasks.Include(s => s.Task).FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new KeyNotFoundException("SubTask not found.");
        await EnsureProjectAccessAsync(subtask.Task.ProjectId, userId);
        _db.SubTasks.Remove(subtask);
        await _db.SaveChangesAsync();
    }

    private async Task EnsureProjectAccessAsync(Guid projectId, Guid userId)
    {
        if (!await _projectClient.IsProjectMemberAsync(projectId, userId))
            throw new UnauthorizedAccessException("You are not a member of this project.");
    }

    private static SubTaskDto MapToDto(SubTask s) => new()
    {
        Id = s.Id, TaskId = s.TaskId, Title = s.Title, IsCompleted = s.IsCompleted,
        AssigneeId = s.AssigneeId, CreatedAt = s.CreatedAt
    };
}
