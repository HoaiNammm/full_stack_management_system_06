using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectService.Models;
using ProjectService.Services;

namespace ProjectService.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/sprints")]
    public class SprintController : ControllerBase
    {
        private readonly SprintService _sprintService;

        public SprintController(SprintService sprintService)
        {
            _sprintService = sprintService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetSprints(Guid projectId)
        {
            var sprints = await _sprintService.GetSprintsByProjectAsync(projectId);
            return Ok(new { success = true, data = sprints });
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSprint(Guid projectId, Guid id)
        {
            var sprint = await _sprintService.GetSprintByIdAsync(id);
            if (sprint == null || sprint.ProjectId != projectId)
                return NotFound(new { success = false, error = new { code = "SPRINT_NOT_FOUND" } });

            return Ok(new { success = true, data = sprint });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateSprint(Guid projectId, [FromBody] CreateSprintRequest request)
        {
            var sprint = new Sprint
            {
                Id          = Guid.NewGuid(),
                ProjectId   = projectId,
                Name        = request.Name,
                Description = request.Description,
                Goal        = request.Goal,
                StartDate   = request.StartDate,
                // EndDate auto-computed in service (StartDate + 14 days)
                Status      = 0,
                CreatedAt   = DateTime.UtcNow
            };

            await _sprintService.CreateSprintAsync(sprint);
            return Ok(new { success = true, data = sprint });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSprint(Guid projectId, Guid id, [FromBody] UpdateSprintRequest request)
        {
            var sprint = await _sprintService.UpdateSprintAsync(
                id, request.Name, request.Description, request.Goal,
                request.StartDate, request.EndDate, request.Status);
            if (sprint == null)
                return NotFound(new { success = false, error = new { code = "SPRINT_NOT_FOUND" } });

            return Ok(new { success = true, data = sprint });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSprint(Guid projectId, Guid id)
        {
            var success = await _sprintService.DeleteSprintAsync(id);
            if (!success)
                return NotFound(new { success = false, error = new { code = "SPRINT_NOT_FOUND" } });

            return Ok(new { success = true });
        }
    }

    public class CreateSprintRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Goal { get; set; }         // Sprint objective
        public DateTime StartDate { get; set; }   // EndDate auto = StartDate + 14 days
    }

    public class UpdateSprintRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Goal { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }           // Set to 1 to publish sprint.started event
    }
}
