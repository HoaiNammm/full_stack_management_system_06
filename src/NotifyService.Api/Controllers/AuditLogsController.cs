using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotifyService.Api.Data;

namespace NotifyService.Api.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuditLogsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs()
    {
        var logs = await _context.AuditLogs
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.UserId,
                x.Action,
                x.EntityName,
                x.EntityId,
                x.OldValuesJson,
                x.NewValuesJson,
                x.IpAddress,
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(logs);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAuditLogById(Guid id)
    {
        var log = await _context.AuditLogs
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.UserId,
                x.Action,
                x.EntityName,
                x.EntityId,
                x.OldValuesJson,
                x.NewValuesJson,
                x.IpAddress,
                x.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (log == null)
        {
            return NotFound(new { message = "KhÃ´ng tÃ¬m tháº¥y audit log" });
        }

        return Ok(log);
    }
}
