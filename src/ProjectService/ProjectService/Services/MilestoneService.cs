using ProjectService.Data;
using ProjectService.Models;
using Microsoft.EntityFrameworkCore;

namespace ProjectService.Services
{
    public class MilestoneService
    {
        private readonly AppDbContext _context;

        public MilestoneService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Milestone>> GetMilestonesByProjectAsync(Guid projectId)
        {
            return await _context.Milestones
                .Where(ms => ms.ProjectId == projectId)
                .OrderBy(ms => ms.TargetDate)
                .ToListAsync();
        }

        public async Task<Milestone?> GetMilestoneByIdAsync(Guid id)
        {
            return await _context.Milestones.FindAsync(id);
        }

        public async Task<Milestone> CreateMilestoneAsync(Milestone milestone)
        {
            _context.Milestones.Add(milestone);
            await _context.SaveChangesAsync();
            return milestone;
        }

        public async Task<Milestone?> UpdateMilestoneAsync(Guid id, string name, string? description, DateTime targetDate, int status)
        {
            var milestone = await _context.Milestones.FindAsync(id);
            if (milestone == null) return null;

            milestone.Name = name;
            milestone.Description = description;
            milestone.TargetDate = targetDate;
            milestone.Status = status;

            await _context.SaveChangesAsync();
            return milestone;
        }

        public async Task<bool> DeleteMilestoneAsync(Guid id)
        {
            var milestone = await _context.Milestones.FindAsync(id);
            if (milestone == null) return false;

            _context.Milestones.Remove(milestone);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
