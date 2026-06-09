using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskService.Services;

namespace TaskService.Controllers;

[ApiController]
[Route("api/kanban-columns")]
public class KanbanController : ControllerBase
{
    private readonly KanbanService             _kanbanService;
    private readonly IProjectMembershipService _membership;

    public KanbanController(KanbanService kanbanService, IProjectMembershipService membership)
    {
        _kanbanService = kanbanService;
        _membership    = membership;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetColumns([FromQuery] Guid projectId)
    {
        if (projectId == Guid.Empty)
            return BadRequest(new { success = false, error = new { code = "MISSING_PROJECT_ID" } });

        var userId = GetCurrentUserId();
        var token  = GetJwtToken();
        if (!await _membership.IsMemberAsync(projectId, userId, token))
            return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });

        await _kanbanService.SeedDefaultColumnsAsync(projectId);

        var cols = await _kanbanService.GetColumnsByProjectAsync(projectId);
        return Ok(new { success = true, data = cols });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddColumn([FromBody] AddColumnRequest request)
    {
        var userId = GetCurrentUserId();
        var token  = GetJwtToken();
        if (!await _membership.IsMemberAsync(request.ProjectId, userId, token))
            return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });

        var col = await _kanbanService.AddColumnAsync(request.ProjectId, request.Name, request.Type ?? "custom");
        return Ok(new { success = true, data = col });
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateColumn(Guid id, [FromBody] UpdateColumnRequest request)
    {
        var col = await _kanbanService.GetColumnByIdAsync(id);
        if (col == null)
            return NotFound(new { success = false, error = new { code = "COLUMN_NOT_FOUND" } });

        var userId = GetCurrentUserId();
        var token  = GetJwtToken();
        if (!await _membership.IsMemberAsync(col.ProjectId, userId, token))
            return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });

        var updated = await _kanbanService.UpdateColumnAsync(id, request.Name, request.Type ?? "custom");
        return Ok(new { success = true, data = updated });
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteColumn(Guid id)
    {
        var col = await _kanbanService.GetColumnByIdAsync(id);
        if (col == null)
            return BadRequest(new { success = false, error = new { code = "COLUMN_HAS_TASKS_OR_NOT_FOUND" } });

        var userId = GetCurrentUserId();
        var token  = GetJwtToken();
        if (!await _membership.IsMemberAsync(col.ProjectId, userId, token))
            return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });

        var ok = await _kanbanService.DeleteColumnAsync(id);
        if (!ok)
            return BadRequest(new { success = false, error = new { code = "COLUMN_HAS_TASKS_OR_NOT_FOUND" } });

        return Ok(new { success = true });
    }

    private Guid   GetCurrentUserId() => Guid.Parse(User.FindFirst("sub")!.Value);
    private string GetJwtToken()      =>
        HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
}

public class AddColumnRequest
{
    public Guid    ProjectId { get; set; }
    public string  Name      { get; set; } = string.Empty;
    public string? Type      { get; set; }
}

public class UpdateColumnRequest
{
    public string  Name { get; set; } = string.Empty;
    public string? Type { get; set; }
}
