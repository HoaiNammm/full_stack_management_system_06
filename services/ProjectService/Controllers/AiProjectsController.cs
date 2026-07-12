using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectService.Data;
using ProjectService.DTOs.AiProjects;
using ProjectService.DTOs.Projects;
using ProjectService.DTOs.Sprints;
using ProjectService.HttpClients;
using ProjectService.Models;
using ProjectService.Services;

namespace ProjectService.Controllers;

[ApiController]
[Route("api/workspaces/{workspaceId:guid}/ai-projects")]
[Authorize]
public class AiProjectsController : ControllerBase
{
    private readonly IAiProjectPlannerService _planner;
    private readonly IProjectService _projectService;
    private readonly IWorkspaceService _workspaceService;
    private readonly TaskServiceClient _taskClient;
    private readonly ProjectDbContext _db;

    public AiProjectsController(
        IAiProjectPlannerService planner,
        IProjectService projectService,
        IWorkspaceService workspaceService,
        TaskServiceClient taskClient,
        ProjectDbContext db)
    {
        _planner = planner;
        _projectService = projectService;
        _workspaceService = workspaceService;
        _taskClient = taskClient;
        _db = db;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("preview")]
    public async Task<IActionResult> Preview(Guid workspaceId, [FromBody] AiPreviewRequest request)
    {
        var team = await GetTeamContextAsync(workspaceId);
        var plan = await _planner.GeneratePlanAsync(request.Prompt, team);
        return Ok(plan);
    }

    [HttpPost("refine")]
    public async Task<IActionResult> Refine(Guid workspaceId, [FromBody] AiRefineRequest request)
    {
        var team = await GetTeamContextAsync(workspaceId);
        var plan = await _planner.RefinePlanAsync(request.Plan, request.Refinement, team);
        return Ok(plan);
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm(Guid workspaceId, [FromBody] AiConfirmRequest request)
    {
        var plan = request.Plan;
        var userId = CurrentUserId;
        var authHeader = Request.Headers.Authorization.ToString();
        var members = await _workspaceService.GetMembersAsync(workspaceId, userId);
        var nameToUserId = members
            .Where(m => m.User is not null)
            .GroupBy(m => m.User!.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().UserId, StringComparer.OrdinalIgnoreCase);

        var project = await _projectService.CreateAsync(workspaceId, new CreateProjectRequest
        {
            Name = plan.Name,
            Description = plan.Description,
            Status = "Planning",
            Priority = "Medium",
            StartDate = plan.StartDate,
            EndDate = plan.EndDate
        }, userId);

        foreach (var milestone in plan.Milestones)
        {
            _db.Milestones.Add(new Milestone
            {
                ProjectId = project.Id,
                Name = milestone.Name,
                Description = milestone.Description,
                DueDate = milestone.DueDate
            });
        }
        if (plan.Milestones.Count > 0) await _db.SaveChangesAsync();

        var sprintIds = new List<Guid>();
        foreach (var sprint in plan.Sprints)
        {
            var created = await _projectService.CreateSprintAsync(project.Id, new CreateSprintRequest
            {
                Name = sprint.Name,
                Goal = sprint.Goal,
                StartDate = sprint.StartDate
            }, userId);
            sprintIds.Add(created.Id);
        }

        foreach (var task in plan.Tasks)
        {
            var priorityLabel = task.Priority switch { 0 => "Low", 1 => "Medium", _ => "High" };
            Guid? sprintId = task.SprintIndex is int idx && idx >= 0 && idx < sprintIds.Count ? sprintIds[idx] : null;
            Guid? assigneeId = task.AssigneeName is not null && nameToUserId.TryGetValue(task.AssigneeName.Trim(), out var uid)
                ? uid
                : null;

            var taskId = await _taskClient.CreateTaskAsync(authHeader, project.Id, new
            {
                title = task.Title,
                priority = priorityLabel,
                sprintId,
                assigneeId,
                estimatedHours = task.EstimatedHours
            });

            foreach (var subtask in task.Subtasks)
                await _taskClient.CreateSubtaskAsync(authHeader, taskId, subtask);
        }

        return Ok(new AiConfirmResultDto { ProjectId = project.Id });
    }

    private async Task<List<AiTeamMemberInfo>> GetTeamContextAsync(Guid workspaceId)
    {
        var members = await _workspaceService.GetMembersAsync(workspaceId, CurrentUserId);
        return members
            .Where(m => m.User is not null)
            .Select(m => new AiTeamMemberInfo(m.User!.Name, m.Role))
            .ToList();
    }
}
