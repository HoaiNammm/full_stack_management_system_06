using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using TaskService.DTOs.TimeLogs;
using TaskService.HttpClients;
using TaskService.Models;

namespace TaskService.Services;

public class TimeLogServiceImpl : ITimeLogService
{
    private readonly TaskDbContext _db;
    private readonly WorkspaceServiceClient _projectClient;

    public TimeLogServiceImpl(TaskDbContext db, WorkspaceServiceClient projectClient)
    {
        _db = db;
        _projectClient = projectClient;
    }

    public async Task<List<TimeLogDto>> GetByTaskAsync(Guid taskId, Guid userId)
    {
        var task = await _db.Tasks.FindAsync(taskId) ?? throw new KeyNotFoundException("Task not found.");
        await EnsureAccessAsync(task.ProjectId, userId);
        var logs = await _db.TimeLogs.Where(l => l.TaskId == taskId).OrderByDescending(l => l.LoggedAt).ToListAsync();
        return logs.Select(MapToDto).ToList();
    }

    public async Task<TimeLogDto> LogAsync(Guid taskId, CreateTimeLogRequest request, Guid userId)
    {
        var task = await _db.Tasks.FindAsync(taskId) ?? throw new KeyNotFoundException("Task not found.");
        await EnsureAccessAsync(task.ProjectId, userId);
        var log = new TimeLog
        {
            TaskId = taskId,
            UserId = userId,
            Description = request.Description,
            HoursLogged = request.HoursLogged,
            LoggedAt = request.LoggedAt ?? DateTime.UtcNow
        };
        _db.TimeLogs.Add(log);
        await _db.SaveChangesAsync();
        return MapToDto(log);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var log = await _db.TimeLogs.Include(l => l.Task).FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException("Time log not found.");
        if (log.UserId != userId) throw new UnauthorizedAccessException("Only the log owner can delete it.");
        _db.TimeLogs.Remove(log);
        await _db.SaveChangesAsync();
    }

    private async Task EnsureAccessAsync(Guid projectId, Guid userId)
    {
        if (!await _projectClient.IsProjectMemberAsync(projectId, userId))
            throw new UnauthorizedAccessException("You are not a member of this project.");
    }

    private static TimeLogDto MapToDto(TimeLog l) => new()
    {
        Id = l.Id, TaskId = l.TaskId, UserId = l.UserId, Description = l.Description,
        HoursLogged = l.HoursLogged, LoggedAt = l.LoggedAt, CreatedAt = l.CreatedAt
    };
}
