using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectService.Models;
using ProjectService.Services;

namespace ProjectService.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/milestones")]
    public class MilestoneController : ControllerBase
    {
        private readonly MilestoneService _milestoneService;
        private readonly MemberService    _memberService;

        public MilestoneController(MilestoneService milestoneService, MemberService memberService)
        {
            _milestoneService = milestoneService;
            _memberService    = memberService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetMilestones(Guid projectId)
        {
            var userId = GetCurrentUserId();
            var role   = await _memberService.GetUserRoleInProjectAsync(projectId, userId);
            if (role == null)
                return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });

            var milestones = await _milestoneService.GetMilestonesByProjectAsync(projectId);
            return Ok(new { success = true, data = milestones });
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMilestone(Guid projectId, Guid id)
        {
            var userId = GetCurrentUserId();
            var role   = await _memberService.GetUserRoleInProjectAsync(projectId, userId);
            if (role == null)
                return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });

            var milestone = await _milestoneService.GetMilestoneByIdAsync(id);
            if (milestone == null || milestone.ProjectId != projectId)
                return NotFound(new { success = false, error = new { code = "MILESTONE_NOT_FOUND" } });

            return Ok(new { success = true, data = milestone });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateMilestone(Guid projectId,
            [FromBody] CreateMilestoneRequest request)
        {
            var userId = GetCurrentUserId();
            var role   = await _memberService.GetUserRoleInProjectAsync(projectId, userId);

            // Owner (0) or Manager (1) can create milestones
            if (role == null || role > 1)
                return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });

            var milestone = new Milestone
            {
                Id          = Guid.NewGuid(),
                ProjectId   = projectId,
                Name        = request.Name,
                Description = request.Description,
                TargetDate  = request.TargetDate,
                Status      = 0,
                CreatedAt   = DateTime.UtcNow
            };

            await _milestoneService.CreateMilestoneAsync(milestone);
            return Ok(new { success = true, data = milestone });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMilestone(Guid projectId, Guid id,
            [FromBody] UpdateMilestoneRequest request)
        {
            var userId = GetCurrentUserId();
            var role   = await _memberService.GetUserRoleInProjectAsync(projectId, userId);

            // Owner (0) or Manager (1) can update milestones
            if (role == null || role > 1)
                return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });

            var milestone = await _milestoneService.UpdateMilestoneAsync(
                id, request.Name, request.Description, request.TargetDate, request.Status);
            if (milestone == null)
                return NotFound(new { success = false, error = new { code = "MILESTONE_NOT_FOUND" } });

            return Ok(new { success = true, data = milestone });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMilestone(Guid projectId, Guid id)
        {
            var userId = GetCurrentUserId();
            var role   = await _memberService.GetUserRoleInProjectAsync(projectId, userId);

            // Owner (0) or Manager (1) can delete milestones
            if (role == null || role > 1)
                return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });

            var success = await _milestoneService.DeleteMilestoneAsync(id);
            if (!success)
                return NotFound(new { success = false, error = new { code = "MILESTONE_NOT_FOUND" } });

            return Ok(new { success = true });
        }

        private Guid GetCurrentUserId() =>
            Guid.Parse(User.FindFirst("sub")!.Value);
    }

    public class CreateMilestoneRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime TargetDate { get; set; }
    }

    public class UpdateMilestoneRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime TargetDate { get; set; }
        public int Status { get; set; }
    }
}
