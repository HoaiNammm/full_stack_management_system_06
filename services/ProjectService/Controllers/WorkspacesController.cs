using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectService.DTOs.Members;
using ProjectService.DTOs.Workspaces;
using ProjectService.Services;

namespace ProjectService.Controllers;

[ApiController]
[Route("api/workspaces")]
[Authorize]
public class WorkspacesController : ControllerBase
{
    private readonly IWorkspaceService _workspaceService;

    public WorkspacesController(IWorkspaceService workspaceService) => _workspaceService = workspaceService;

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _workspaceService.GetAllByUserAsync(CurrentUserId));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var workspace = await _workspaceService.GetByIdAsync(id, CurrentUserId);
        return workspace is null ? NotFound() : Ok(workspace);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkspaceRequest request)
    {
        var workspace = await _workspaceService.CreateAsync(request, CurrentUserId);
        return CreatedAtAction(nameof(GetById), new { id = workspace.Id }, workspace);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWorkspaceRequest request) =>
        Ok(await _workspaceService.UpdateAsync(id, request, CurrentUserId));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _workspaceService.DeleteAsync(id, CurrentUserId);
        return NoContent();
    }

    [HttpGet("{id:guid}/members")]
    public async Task<IActionResult> GetMembers(Guid id) =>
        Ok(await _workspaceService.GetMembersAsync(id, CurrentUserId));

    [HttpPost("{id:guid}/members")]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddWorkspaceMemberRequest request) =>
        Ok(await _workspaceService.AddMemberAsync(id, request, CurrentUserId));

    [HttpPut("{id:guid}/members/{memberId:guid}")]
    public async Task<IActionResult> UpdateMemberRole(Guid id, Guid memberId, [FromBody] UpdateMemberRoleRequest request) =>
        Ok(await _workspaceService.UpdateMemberRoleAsync(id, memberId, request, CurrentUserId));

    [HttpDelete("{id:guid}/members/{memberId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid memberId)
    {
        await _workspaceService.RemoveMemberAsync(id, memberId, CurrentUserId);
        return NoContent();
    }
}
