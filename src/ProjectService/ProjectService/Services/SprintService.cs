using ProjectService.Data;
using ProjectService.Models;
using ProjectService.Events;
using Microsoft.EntityFrameworkCore;

namespace ProjectService.Services
{
    public class SprintService
    {
        private readonly AppDbContext _context;
        private readonly IEventPublisher _eventPublisher;

        public SprintService(AppDbContext context, IEventPublisher eventPublisher)
        {
            _context        = context;
            _eventPublisher = eventPublisher;
        }

        public async Task<List<Sprint>> GetSprintsByProjectAsync(Guid projectId)
        {
            return await _context.Sprints
                .Where(s => s.ProjectId == projectId)
                .OrderBy(s => s.StartDate)
                .ToListAsync();
        }

        public async Task<Sprint?> GetSprintByIdAsync(Guid id)
        {
            return await _context.Sprints.FindAsync(id);
        }

        public async Task<Sprint> CreateSprintAsync(Sprint sprint)
        {
            // Auto-calculate EndDate = StartDate + 14 days (2 weeks)
            sprint.EndDate = sprint.StartDate.AddDays(14);

            _context.Sprints.Add(sprint);
            await _context.SaveChangesAsync();
            return sprint;
        }

        public async Task<Sprint?> UpdateSprintAsync(Guid id, string name, string? description,
            string? goal, DateTime startDate, DateTime endDate, int status)
        {
            var sprint = await _context.Sprints.FindAsync(id);
            if (sprint == null) return null;

            var wasNotStarted = sprint.Status != 1; // not Active yet
            sprint.Name        = name;
            sprint.Description = description;
            sprint.Goal        = goal;
            sprint.StartDate   = startDate;
            sprint.EndDate     = endDate;
            sprint.Status      = status;
            sprint.UpdatedAt   = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Publish sprint.started when status transitions to Active (1)
            if (status == 1 && wasNotStarted)
            {
                await _eventPublisher.PublishAsync("sprint.started", new SprintStartedEvent
                {
                    SprintId   = sprint.Id,
                    ProjectId  = sprint.ProjectId,
                    SprintName = sprint.Name,
                    Goal       = sprint.Goal,
                    StartDate  = sprint.StartDate,
                    EndDate    = sprint.EndDate
                });
            }

            return sprint;
        }

        public async Task<bool> DeleteSprintAsync(Guid id)
        {
            var sprint = await _context.Sprints.FindAsync(id);
            if (sprint == null) return false;

            _context.Sprints.Remove(sprint);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
