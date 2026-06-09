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
        private readonly SprintService  _sprintService;
        private readonly MemberService  _memberService;

        public SprintController(SprintService sprintService, MemberService memberService)
        {
            _sprintService = sprintService;
            _memberService = memberService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetSprints(Guid projectId)
        {
            var userId = GetCurrentUserId();
            var role   = await _memberService.GetUserRoleInProjectAsync(projectId, userId);
            if (role == null)
                return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });

            var sprints = await _sprintService.GetSprintsByProjectAsync(projectId);
            return Ok(new { success = true, data = sprints });
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSprint(Guid projectId, Guid id)
        {
            var userId = GetCurrentUserId();
            var role   = await _memberService.GetUserRoleInProjectAsync(projectId, userId);
            if (role == null)
                return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });

            var sprint = await _sprintService.GetSprintByIdAsync(id);
            if (sprint == null || sprint.ProjectId != projectId)
                return NotFound(new { success = false, error = new { code = "SPRINT_NOT_FOUND" } });

            return Ok(new { success = true, data = sprint });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateSprint(Guid projectId, [FromBody] CreateSprintRequest request)
        {
            var userId = GetCurrentUserId();
            var role   = await _memberService.GetUserRoleInProjectAsync(projectId, userId);

            // Owner (0) or Manager (1) can create sprints
            if (role == null || role > 1)
                return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });

            var sprint = new Sprint
            {
                Id          = Guid.NewGuid(),
                ProjectId   = projectId,
                Name        = request.Name,
                Description = request.Description,
                Goal        = request.Goal,
                StartDate   = request.StartDate,
                Status      = 0,
                CreatedAt   = DateTime.UtcNow
            };

            await _sprintService.CreateSprintAsync(sprint);
            return Ok(new { success = true, data = sprint });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSprint(Guid projectId, Guid id,
            [FromBody] UpdateSprintRequest request)
        {
            var userId = GetCurrentUserId();
            var role   = await _memberService.GetUserRoleInProjectAsync(projectId, userId);

            // Owner (0) or Manager (1) can update sprints
            if (role == null || role > 1)
                return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });

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
            var userId = GetCurrentUserId();
            var role   = await _memberService.GetUserRoleInProjectAsync(projectId, userId);

            // Owner (0) or Manager (1) can delete sprints
            if (role == null || role > 1)
                return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });

            var success = await _sprintService.DeleteSprintAsync(id);
            if (!success)
                return NotFound(new { success = false, error = new { code = "SPRINT_NOT_FOUND" } });

            return Ok(new { success = true });
        }

        private Guid GetCurrentUserId() =>
            Guid.Parse(User.FindFirst("sub")!.Value);
    }

    public class CreateSprintRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Goal { get; set; }
        public DateTime StartDate { get; set; }
    }

    public class UpdateSprintRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Goal { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }
    }
}
