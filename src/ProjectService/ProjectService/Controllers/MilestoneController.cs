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

        public MilestoneController(MilestoneService milestoneService)
        {
            _milestoneService = milestoneService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetMilestones(Guid projectId)
        {
            var milestones = await _milestoneService.GetMilestonesByProjectAsync(projectId);
            return Ok(new { success = true, data = milestones });
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMilestone(Guid projectId, Guid id)
        {
            var milestone = await _milestoneService.GetMilestoneByIdAsync(id);
            if (milestone == null || milestone.ProjectId != projectId)
                return NotFound(new { success = false, error = new { code = "MILESTONE_NOT_FOUND" } });

            return Ok(new { success = true, data = milestone });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateMilestone(Guid projectId, [FromBody] CreateMilestoneRequest request)
        {
            var milestone = new Milestone
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Name = request.Name,
                Description = request.Description,
                TargetDate = request.TargetDate,
                Status = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _milestoneService.CreateMilestoneAsync(milestone);
            return Ok(new { success = true, data = milestone });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMilestone(Guid projectId, Guid id, [FromBody] UpdateMilestoneRequest request)
        {
            var milestone = await _milestoneService.UpdateMilestoneAsync(id, request.Name, request.Description, request.TargetDate, request.Status);
            if (milestone == null)
                return NotFound(new { success = false, error = new { code = "MILESTONE_NOT_FOUND" } });

            return Ok(new { success = true, data = milestone });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMilestone(Guid projectId, Guid id)
        {
            var success = await _milestoneService.DeleteMilestoneAsync(id);
            if (!success)
                return NotFound(new { success = false, error = new { code = "MILESTONE_NOT_FOUND" } });

            return Ok(new { success = true });
        }
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
