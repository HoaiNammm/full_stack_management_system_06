using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using TaskService.Models;

namespace TaskService.Services
{
    public class SubTaskService
    {
        private readonly AppDbContext _context;

        public SubTaskService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SubTask>> GetSubTasksByTaskAsync(Guid taskId)
        {
            return await _context.SubTasks
                .Where(s => s.TaskId == taskId)
                .OrderBy(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<SubTask?> GetSubTaskByIdAsync(Guid id)
        {
            return await _context.SubTasks.FindAsync(id);
        }

        public async Task<SubTask> CreateSubTaskAsync(SubTask subTask)
        {
            _context.SubTasks.Add(subTask);
            await _context.SaveChangesAsync();
            return subTask;
        }

        public async Task<SubTask?> UpdateSubTaskAsync(Guid id, string title, string? description,
            Guid? assignedTo, int status, decimal? estimatedHours)
        {
            var subTask = await _context.SubTasks.FindAsync(id);
            if (subTask == null) return null;

            subTask.Title          = title;
            subTask.Description    = description;
            subTask.AssignedTo     = assignedTo;
            subTask.Status         = status;
            subTask.EstimatedHours = estimatedHours;
            subTask.UpdatedAt      = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return subTask;
        }

        public async Task<bool> DeleteSubTaskAsync(Guid id)
        {
            var subTask = await _context.SubTasks.FindAsync(id);
            if (subTask == null) return false;

            _context.SubTasks.Remove(subTask);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
