using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectService.Models;
using ProjectService.Services;
using System.Security.Claims;

namespace ProjectService.Controllers
{
    [ApiController]
    [Route("api/projects")]
    public class ProjectController : ControllerBase
    {
        private readonly ProjectService.Services.ProjectService _projectService;

        public ProjectController(ProjectService.Services.ProjectService projectService)
        {
            _projectService = projectService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            var projects = await _projectService.GetAllProjectsAsync();
            return Ok(new { success = true, data = projects });
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(Guid id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
                return NotFound(new { success = false, error = new { code = "PROJECT_NOT_FOUND" } });

            return Ok(new { success = true, data = project });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
        {
            var userId = User.FindFirst("sub")?.Value;

            var project = new Project
            {
                Id          = Guid.NewGuid(),
                Name        = request.Name,
                Description = request.Description,
                Status      = 0,
                Color       = request.Color,
                StartDate   = request.StartDate,
                EndDate     = request.EndDate,
                CreatedBy   = Guid.Parse(userId!),
                CreatedAt   = DateTime.UtcNow
            };

            await _projectService.CreateProjectAsync(project);
            return CreatedAtAction(nameof(GetProject), new { id = project.Id },
                new { success = true, data = project });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request)
        {
            var project = await _projectService.UpdateProjectAsync(
                id, request.Name, request.Description, request.Status,
                request.Color, request.StartDate, request.EndDate);
            if (project == null)
                return NotFound(new { success = false, error = new { code = "PROJECT_NOT_FOUND" } });

            return Ok(new { success = true, data = project });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            var success = await _projectService.DeleteProjectAsync(id);
            if (!success)
                return NotFound(new { success = false, error = new { code = "PROJECT_NOT_FOUND" } });

            return Ok(new { success = true });
        }
    }

    public class CreateProjectRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }       // Hex, e.g. "#4F46E5"
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class UpdateProjectRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Status { get; set; }
        public string? Color { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}