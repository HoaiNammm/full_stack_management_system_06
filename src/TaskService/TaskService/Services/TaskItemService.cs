using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using TaskService.Events;
using TaskService.Models;

namespace TaskService.Services;

public class TaskItemService
{
    private readonly AppDbContext      _db;
    private readonly IEventPublisher   _events;

    public TaskItemService(AppDbContext db, IEventPublisher events)
    {
        _db     = db;
        _events = events;
    }

    public async Task<List<TaskItem>> GetAllAsync(
        Guid? projectId  = null,
        Guid? columnId   = null,
        Guid? assignedTo = null,
        Guid? sprintId   = null)
    {
        var q = _db.Tasks.Where(t => t.DeletedAt == null);

        if (projectId.HasValue)  q = q.Where(t => t.ProjectId  == projectId.Value);
        if (columnId.HasValue)   q = q.Where(t => t.ColumnId   == columnId.Value);
        if (assignedTo.HasValue) q = q.Where(t => t.AssignedTo == assignedTo.Value);
        if (sprintId.HasValue)   q = q.Where(t => t.SprintId   == sprintId.Value);

        return await q.OrderBy(t => t.CreatedAt).ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id)
        => await _db.Tasks
                    .Include(t => t.SubTasks.Where(s => s.DeletedAt == null))
                    .FirstOrDefaultAsync(t => t.Id == id && t.DeletedAt == null);

    public async Task<TaskItem> CreateAsync(TaskItem task, Guid createdBy)
    {
        task.Id        = Guid.NewGuid();
        task.CreatedAt = DateTime.UtcNow;
        task.CreatedBy = createdBy;
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
        return task;
    }

    public async Task<TaskItem?> UpdateAsync(Guid id, string title, string? description,
        int priority, Guid? assignedTo, DateTime? dueDate, decimal? estimatedHours,
        Guid? sprintId = null, bool clearSprint = false)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null || task.DeletedAt != null) return null;

        task.Title          = title;
        task.Description    = description;
        task.Priority       = priority;
        task.AssignedTo     = assignedTo;
        task.DueDate        = dueDate;
        task.EstimatedHours = estimatedHours;
        if (sprintId.HasValue) task.SprintId = sprintId.Value;
        if (clearSprint)       task.SprintId = null;
        await _db.SaveChangesAsync();
        return task;
    }

    public async Task<TaskItem?> MoveToColumnAsync(Guid id, Guid newColumnId, Guid movedBy)
    {
        var task = await _db.Tasks
                            .FirstOrDefaultAsync(t => t.Id == id && t.DeletedAt == null);
        if (task == null) return null;

        var newCol = await _db.KanbanColumns.FindAsync(newColumnId);
        if (newCol == null) return null;

        var oldColumnId = task.ColumnId;
        task.ColumnId   = newColumnId;
        await _db.SaveChangesAsync();

        await _events.PublishAsync("task_events", "task.column.changed", new TaskColumnChangedEvent
        {
            TaskId        = task.Id,
            ProjectId     = task.ProjectId,
            TaskTitle     = task.Title,
            NewColumnType = newCol.Type,
            OldColumnId   = oldColumnId,
            NewColumnId   = newColumnId,
            AssignedTo    = task.AssignedTo,
            ChangedBy     = movedBy,
            ChangedAt     = DateTime.UtcNow
        });

        return task;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null || task.DeletedAt != null) return false;
        task.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }
}
