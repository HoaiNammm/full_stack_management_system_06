using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using TaskService.Models;

namespace TaskService.Services
{
    public class TaskTimeLogService
    {
        private readonly AppDbContext _context;

        public TaskTimeLogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TaskTimeLog>> GetLogsByTaskAsync(Guid taskId)
        {
            return await _context.TaskTimeLogs
                .Where(l => l.TaskId == taskId)
                .OrderByDescending(l => l.LoggedDate)
                .ToListAsync();
        }

        public async Task<TaskTimeLog> CreateLogAsync(TaskTimeLog log)
        {
            _context.TaskTimeLogs.Add(log);
            await _context.SaveChangesAsync();
            return log;
        }

        public async Task<bool> DeleteLogAsync(Guid id)
        {
            var log = await _context.TaskTimeLogs.FindAsync(id);
            if (log == null) return false;

            _context.TaskTimeLogs.Remove(log);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
