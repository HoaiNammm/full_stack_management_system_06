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
            var members = await _memberService.GetProjectMembersAsync(projectId);
            return Ok(new { success = true, data = members });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddMember(Guid projectId, [FromBody] AddMemberRequest request)
        {
            var member = new Member
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                UserId = request.UserId,
                Role = request.Role,
                JoinedAt = DateTime.UtcNow
            };

            await _memberService.AddMemberAsync(member);
            return Ok(new { success = true, data = member });
        }

        [Authorize]
        [HttpPut("{memberId}/role")]
        public async Task<IActionResult> UpdateMemberRole(Guid projectId, Guid memberId, [FromBody] UpdateMemberRoleRequest request)
        {
            var member = await _memberService.UpdateMemberRoleAsync(memberId, request.Role);
            if (member == null)
                return NotFound(new { success = false, error = new { code = "MEMBER_NOT_FOUND" } });

            return Ok(new { success = true, data = member });
        }

        [Authorize]
        [HttpDelete("{memberId}")]
        public async Task<IActionResult> RemoveMember(Guid memberId)
        {
            var success = await _memberService.RemoveMemberAsync(memberId);
            if (!success)
                return NotFound(new { success = false, error = new { code = "MEMBER_NOT_FOUND" } });

            return Ok(new { success = true });
        }
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