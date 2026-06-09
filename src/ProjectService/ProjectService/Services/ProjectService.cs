using ProjectService.Data;
using ProjectService.Models;
using Microsoft.EntityFrameworkCore;

namespace ProjectService.Services
{
    public class ProjectService
    {
        private readonly AppDbContext _context;

        public ProjectService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects.ToListAsync();
        }

        public async Task<List<Project>> GetUserProjectsAsync(Guid userId)
        {
            return await _context.Projects
                .Where(p => p.Members.Any(m => m.UserId == userId))
                .ToListAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(Guid id)
        {
            return await _context.Projects
                .Include(p => p.Members)
                .Include(p => p.Sprints)
                .Include(p => p.Milestones)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Project> CreateProjectAsync(Project project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<Project?> UpdateProjectAsync(Guid id, string name, string? description,
            int status, string? color, DateTime? startDate, DateTime? endDate)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return null;

            project.Name        = name;
            project.Description = description;
            project.Status      = status;
            project.Color       = color;
            project.StartDate   = startDate;
            project.EndDate     = endDate;
            project.UpdatedAt   = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<bool> DeleteProjectAsync(Guid id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return false;

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}