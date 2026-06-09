using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectService.Models;
using ProjectService.Services;

namespace ProjectService.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/members")]
    public class MemberController : ControllerBase
    {
        private readonly MemberService _memberService;

        public MemberController(MemberService memberService)
        {
            _memberService = memberService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetMembers(Guid projectId)
        {
            var userId = GetCurrentUserId();
            var role   = await _memberService.GetUserRoleInProjectAsync(projectId, userId);
            if (role == null)
                return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });

            var members = await _memberService.GetProjectMembersAsync(projectId);
            return Ok(new { success = true, data = members });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddMember(Guid projectId, [FromBody] AddMemberRequest request)
        {
            var userId = GetCurrentUserId();
            var role   = await _memberService.GetUserRoleInProjectAsync(projectId, userId);

            // Owner (0) or Manager (1) can add members
            if (role == null || role > 1)
                return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });

            var member = new Member
            {
                Id        = Guid.NewGuid(),
                ProjectId = projectId,
                UserId    = request.UserId,
                Role      = request.Role,
                JoinedAt  = DateTime.UtcNow
            };

            await _memberService.AddMemberAsync(member);
            return Ok(new { success = true, data = member });
        }

        [Authorize]
        [HttpPut("{memberId}/role")]
        public async Task<IActionResult> UpdateMemberRole(Guid projectId, Guid memberId,
            [FromBody] UpdateMemberRoleRequest request)
        {
            var userId = GetCurrentUserId();
            var role   = await _memberService.GetUserRoleInProjectAsync(projectId, userId);

            // Only Owner (0) can change roles
            if (role == null || role != 0)
                return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });

            var member = await _memberService.UpdateMemberRoleAsync(memberId, request.Role);
            if (member == null)
                return NotFound(new { success = false, error = new { code = "MEMBER_NOT_FOUND" } });

            return Ok(new { success = true, data = member });
        }

        [Authorize]
        [HttpDelete("{memberId}")]
        public async Task<IActionResult> RemoveMember(Guid projectId, Guid memberId)
        {
            var userId = GetCurrentUserId();
            var role   = await _memberService.GetUserRoleInProjectAsync(projectId, userId);

            // Only Owner (0) can remove members
            if (role == null || role != 0)
                return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });

            var success = await _memberService.RemoveMemberAsync(memberId);
            if (!success)
                return NotFound(new { success = false, error = new { code = "MEMBER_NOT_FOUND" } });

            return Ok(new { success = true });
        }

        private Guid GetCurrentUserId() =>
            Guid.Parse(User.FindFirst("sub")!.Value);
    }

    public class AddMemberRequest
    {
        public Guid UserId { get; set; }
        public int Role { get; set; }  // Owner=0, Manager=1, Member=2, Viewer=3
    }

    public class UpdateMemberRoleRequest
    {
        public int Role { get; set; }  // Owner=0, Manager=1, Member=2, Viewer=3
    }
}
