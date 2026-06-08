using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotifyService.Api.Data;
using NotifyService.Api.Models;

namespace NotifyService.Api.Controllers;

[ApiController]
[Route("api/system-logs")]
[Authorize]
public class SystemLogsController : ControllerBase
{
    private readonly NotifyDbContext _context;

    public SystemLogsController(NotifyDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetSystemLogs()
    {
        var logs = await _context.SystemLogs
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.Level,
                x.Source,
                x.Message,
                x.Exception,
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(logs);
    }

    [HttpPost("test")]
    public async Task<IActionResult> CreateTestSystemLog()
    {
        var log = new SystemLog
        {
            Id = Guid.NewGuid(),
            Level = "Information",
            Source = "NotifyService.Api",
            Message = "This is a test system log",
            Exception = null,
            CreatedAt = DateTime.UtcNow
        };

        _context.SystemLogs.Add(log);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "System log created",
            log.Id
        });
    }
}