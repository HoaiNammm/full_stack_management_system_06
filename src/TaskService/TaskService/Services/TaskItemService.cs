using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using TaskService.Events;
using TaskService.Models;

namespace TaskService.Services
{
    public class TaskItemService
    {
        private readonly AppDbContext _context;
        private readonly IEventPublisher _eventPublisher;

        public TaskItemService(AppDbContext context, IEventPublisher eventPublisher)
        {
            _context        = context;
            _eventPublisher = eventPublisher;
        }

        public async Task<List<TaskItem>> GetTasksAsync(Guid? projectId, Guid? sprintId, Guid? assignedTo, Guid? columnId)
        {
            var query = _context.Tasks
                .Where(t => t.DeletedAt == null)
                .AsQueryable();

            if (projectId.HasValue)  query = query.Where(t => t.ProjectId == projectId.Value);
            if (sprintId.HasValue)   query = query.Where(t => t.SprintId == sprintId.Value);
            if (assignedTo.HasValue) query = query.Where(t => t.AssignedTo == assignedTo.Value);
            if (columnId.HasValue)   query = query.Where(t => t.ColumnId == columnId.Value);

            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }

        public async Task<TaskItem?> GetTaskByIdAsync(Guid id)
        {
            return await _context.Tasks
                .Include(t => t.Column)
                .Include(t => t.SubTasks.Where(s => s.DeletedAt == null))
                .Include(t => t.TimeLogs)
                .Include(t => t.AssignmentHistory)
                .FirstOrDefaultAsync(t => t.Id == id && t.DeletedAt == null);
        }

        public async Task<TaskItem> CreateTaskAsync(TaskItem task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<TaskItem?> UpdateTaskAsync(Guid id, string title, string? description,
            Guid? sprintId, int priority, decimal? estimatedHours, DateTime? deadline)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.DeletedAt == null);
            if (task == null) return null;

            task.Title          = title;
            task.Description    = description;
            task.SprintId       = sprintId;
            task.Priority       = priority;
            task.EstimatedHours = estimatedHours;
            task.Deadline       = deadline;
            task.UpdatedAt      = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<TaskItem?> MoveToColumnAsync(Guid id, Guid newColumnId, Guid movedBy)
        {
            var task = await _context.Tasks
                .Include(t => t.Column)
                .FirstOrDefaultAsync(t => t.Id == id && t.DeletedAt == null);
            if (task == null) return null;

            var newColumn = await _context.KanbanColumns.FindAsync(newColumnId);
            if (newColumn == null) return null;

            var oldColumnId = task.ColumnId;
            task.ColumnId  = newColumnId;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _eventPublisher.PublishAsync("task.column.changed", new TaskColumnChangedEvent
            {
                TaskId        = task.Id,
                ProjectId     = task.ProjectId,
                TaskTitle     = task.Title,
                OldColumnId   = oldColumnId,
                NewColumnId   = newColumnId,
                NewColumnType = newColumn.Type,
                AssignedTo    = task.AssignedTo,
                ChangedBy     = movedBy,
                ChangedAt     = DateTime.UtcNow
            });

            return task;
        }

        public async Task<TaskItem?> AssignTaskAsync(Guid id, Guid? newAssignee, Guid assignedBy)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.DeletedAt == null);
            if (task == null) return null;

            var previousAssignee = task.AssignedTo;
            task.AssignedTo = newAssignee;
            task.UpdatedAt  = DateTime.UtcNow;

            _context.TaskAssignmentHistories.Add(new TaskAssignmentHistory
            {
                Id               = Guid.NewGuid(),
                TaskId           = task.Id,
                PreviousAssignee = previousAssignee,
                NewAssignee      = newAssignee,
                ChangedBy        = assignedBy,
                ChangedAt        = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            await _eventPublisher.PublishAsync("task.assigned", new TaskAssignedEvent
            {
                TaskId           = task.Id,
                ProjectId        = task.ProjectId,
                TaskTitle        = task.Title,
                PreviousAssignee = previousAssignee,
                NewAssignee      = newAssignee,
                AssignedBy       = assignedBy,
                AssignedAt       = DateTime.UtcNow
            });

            return task;
        }

        // Soft delete
        public async Task<bool> DeleteTaskAsync(Guid id)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.DeletedAt == null);
            if (task == null) return false;

            task.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
