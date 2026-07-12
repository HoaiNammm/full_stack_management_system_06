using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectService.Data;
using ProjectService.DTOs.Milestones;
using ProjectService.Models;

namespace ProjectService.Controllers;

[ApiController]
[Route("api/workspaces/{workspaceId:guid}/projects/{projectId:guid}/milestones")]
[Authorize]
public class MilestonesController : ControllerBase
{
    private readonly ProjectDbContext _db;
    public MilestonesController(ProjectDbContext db) => _db = db;
    private Guid Me => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task<bool> IsMemberAsync(Guid projectId) =>
        await _db.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == Me);

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid workspaceId, Guid projectId)
    {
        if (!await IsMemberAsync(projectId)) return Forbid();
        var list = await _db.Milestones
            .Where(m => m.ProjectId == projectId)
            .OrderBy(m => m.DueDate).ThenBy(m => m.CreatedAt)
            .Select(m => ToDto(m))
            .ToListAsync();
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid workspaceId, Guid projectId, [FromBody] CreateMilestoneRequest req)
    {
        if (!await IsMemberAsync(projectId)) return Forbid();
        if (req.DueDate.HasValue && req.DueDate.Value.Date < DateTime.UtcNow.Date)
            throw new InvalidOperationException("Milestone due date cannot be in the past.");
        var status = Enum.TryParse<MilestoneStatus>(req.Status, out var s) ? s : MilestoneStatus.NotStarted;
        var m = new Milestone
        {
            ProjectId   = projectId,
            Name        = req.Name,
            Description = req.Description,
            DueDate     = req.DueDate,
            Status      = status,
        };
        _db.Milestones.Add(m);
        await _db.SaveChangesAsync();
        return Ok(ToDto(m));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid workspaceId, Guid projectId, Guid id, [FromBody] UpdateMilestoneRequest req)
    {
        if (!await IsMemberAsync(projectId)) return Forbid();
        var m = await _db.Milestones.FirstOrDefaultAsync(x => x.Id == id && x.ProjectId == projectId);
        if (m is null) return NotFound();
        m.Name        = req.Name;
        m.Description = req.Description;
        m.DueDate     = req.DueDate;
        m.Status      = Enum.TryParse<MilestoneStatus>(req.Status, out var s) ? s : m.Status;
        m.UpdatedAt   = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ToDto(m));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid workspaceId, Guid projectId, Guid id)
    {
        if (!await IsMemberAsync(projectId)) return Forbid();
        var m = await _db.Milestones.FirstOrDefaultAsync(x => x.Id == id && x.ProjectId == projectId);
        if (m is null) return NotFound();
        _db.Milestones.Remove(m);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static MilestoneDto ToDto(Milestone m) => new()
    {
        Id          = m.Id,
        ProjectId   = m.ProjectId,
        Name        = m.Name,
        Description = m.Description,
        DueDate     = m.DueDate,
        Status      = m.Status.ToString(),
        CreatedAt   = m.CreatedAt,
        UpdatedAt   = m.UpdatedAt,
    };
}
