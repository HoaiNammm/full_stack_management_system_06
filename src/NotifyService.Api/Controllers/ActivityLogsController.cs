using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotifyService.Api.Data;

namespace NotifyService.Api.Controllers;

[ApiController]
[Route("api/activity-logs")]
[Authorize]
public class ActivityLogsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ActivityLogsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("task/{taskId:guid}")]
    public async Task<IActionResult> GetActivityLogsByTask(Guid taskId)
    {
        var logs = await _context.ActivityLogs
            .Where(x => x.TaskId == taskId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.UserId,
                x.TaskId,
                x.ProjectId,
                x.Action,
                x.Description,
                x.MetadataJson,
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(logs);
    }

    [HttpGet("project/{projectId:guid}")]
    public async Task<IActionResult> GetActivityLogsByProject(Guid projectId)
    {
        var logs = await _context.ActivityLogs
            .Where(x => x.ProjectId == projectId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.UserId,
                x.TaskId,
                x.ProjectId,
                x.Action,
                x.Description,
                x.MetadataJson,
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(logs);
    }
}
