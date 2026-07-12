using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectService.DTOs.Members;
using ProjectService.DTOs.Projects;
using ProjectService.DTOs.Sprints;
using ProjectService.Services;

namespace ProjectService.Controllers;

[ApiController]
[Route("api/workspaces/{workspaceId:guid}/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService) => _projectService = projectService;

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid workspaceId) =>
        Ok(await _projectService.GetByWorkspaceAsync(workspaceId, CurrentUserId));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid workspaceId, Guid id)
    {
        var project = await _projectService.GetByIdAsync(id, CurrentUserId);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid workspaceId, [FromBody] CreateProjectRequest request)
    {
        var project = await _projectService.CreateAsync(workspaceId, request, CurrentUserId);
        return CreatedAtAction(nameof(GetById), new { workspaceId, id = project.Id }, project);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid workspaceId, Guid id, [FromBody] UpdateProjectRequest request) =>
        Ok(await _projectService.UpdateAsync(id, request, CurrentUserId));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid workspaceId, Guid id)
    {
        await _projectService.DeleteAsync(id, CurrentUserId);
        return NoContent();
    }

    [HttpGet("{id:guid}/members")]
    public async Task<IActionResult> GetMembers(Guid workspaceId, Guid id) =>
        Ok(await _projectService.GetMembersAsync(id, CurrentUserId));

    [HttpPost("{id:guid}/members")]
    public async Task<IActionResult> AddMember(Guid workspaceId, Guid id, [FromBody] AddProjectMemberRequest request) =>
        Ok(await _projectService.AddMemberAsync(id, request, CurrentUserId));

    [HttpPut("{id:guid}/members/{memberId:guid}")]
    public async Task<IActionResult> UpdateMemberRole(Guid workspaceId, Guid id, Guid memberId, [FromBody] UpdateMemberRoleRequest request) =>
        Ok(await _projectService.UpdateMemberRoleAsync(id, memberId, request, CurrentUserId));

    [HttpDelete("{id:guid}/members/{memberId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid workspaceId, Guid id, Guid memberId)
    {
        await _projectService.RemoveMemberAsync(id, memberId, CurrentUserId);
        return NoContent();
    }

    [HttpGet("{id:guid}/sprints")]
    public async Task<IActionResult> GetSprints(Guid workspaceId, Guid id) =>
        Ok(await _projectService.GetSprintsAsync(id, CurrentUserId));

    [HttpPost("{id:guid}/sprints")]
    public async Task<IActionResult> CreateSprint(Guid workspaceId, Guid id, [FromBody] CreateSprintRequest request) =>
        Ok(await _projectService.CreateSprintAsync(id, request, CurrentUserId));

    [HttpPut("{id:guid}/sprints/{sprintId:guid}")]
    public async Task<IActionResult> UpdateSprint(Guid workspaceId, Guid id, Guid sprintId, [FromBody] UpdateSprintRequest request) =>
        Ok(await _projectService.UpdateSprintAsync(id, sprintId, request, CurrentUserId));

    [HttpPost("{id:guid}/sprints/{sprintId:guid}/start")]
    public async Task<IActionResult> StartSprint(Guid workspaceId, Guid id, Guid sprintId) =>
        Ok(await _projectService.StartSprintAsync(id, sprintId, CurrentUserId));

    [HttpPost("{id:guid}/sprints/{sprintId:guid}/complete")]
    public async Task<IActionResult> CompleteSprint(Guid workspaceId, Guid id, Guid sprintId) =>
        Ok(await _projectService.CompleteSprintAsync(id, sprintId, CurrentUserId));

    [HttpDelete("{id:guid}/sprints/{sprintId:guid}")]
    public async Task<IActionResult> DeleteSprint(Guid workspaceId, Guid id, Guid sprintId)
    {
        await _projectService.DeleteSprintAsync(id, sprintId, CurrentUserId);
        return NoContent();
    }
}

[ApiController]
[Route("internal/projects")]
public class ProjectsInternalController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsInternalController(IProjectService projectService) => _projectService = projectService;

    [HttpGet("{id:guid}/member-ids")]
    public async Task<IActionResult> GetMemberIds(Guid id) =>
        Ok(await _projectService.GetMemberIdsAsync(id));
}
