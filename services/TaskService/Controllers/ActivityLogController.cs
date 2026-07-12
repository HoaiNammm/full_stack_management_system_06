using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using TaskService.DTOs.ActivityLogs;
using TaskService.HttpClients;

namespace TaskService.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/activity")]
[Authorize]
public class ActivityLogController : ControllerBase
{
    private readonly TaskDbContext _db;
    private readonly WorkspaceServiceClient _workspaceClient;
    private readonly UserServiceClient _userClient;

    public ActivityLogController(TaskDbContext db, WorkspaceServiceClient workspaceClient, UserServiceClient userClient)
    {
        _db = db;
        _workspaceClient = workspaceClient;
        _userClient = userClient;
    }

    private Guid Me => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid projectId)
    {
        var isMember = await _workspaceClient.IsProjectMemberAsync(projectId, Me);
        if (!isMember) return Forbid();

        var logs = await _db.ActivityLogs
            .Where(a => a.ProjectId == projectId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(100)
            .ToListAsync();

        var actorIds = logs.Select(l => l.ActorId).Distinct();
        var users    = await _userClient.GetUsersAsync(actorIds);
        var userMap  = users.ToDictionary(u => u.Id);

        var result = logs.Select(l => new ActivityLogDto
        {
            Id          = l.Id,
            Action      = l.Action,
            EntityName  = l.EntityName,
            Description = l.Description,
            CreatedAt   = l.CreatedAt,
            Actor       = userMap.TryGetValue(l.ActorId, out var u)
                ? new ActorInfo { Id = u.Id, Name = u.Name, AvatarUrl = u.AvatarUrl }
                : new ActorInfo { Id = l.ActorId, Name = "Unknown" },
        });

        return Ok(result);
    }
}
