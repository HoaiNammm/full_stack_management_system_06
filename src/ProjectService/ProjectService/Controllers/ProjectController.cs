using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectService.Models;
using ProjectService.Services;
using System.Security.Claims;

namespace ProjectService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                CreatedBy = Guid.Parse(userId),
                CreatedAt = DateTime.UtcNow
            };

            await _projectService.CreateProjectAsync(project);
            return CreatedAtAction(nameof(GetProject), new { id = project.Id },
                new { success = true, data = project });
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
        public string Name { get; set; }
        public string Description { get; set; }
    }
}