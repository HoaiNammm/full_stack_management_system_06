using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotifyService.Api.Data;
using NotifyService.Api.Models;

namespace NotifyService.Api.Controllers;

[ApiController]
[Route("api/notification-channels")]
[Authorize]
public class NotificationChannelsController : ControllerBase
{
    private readonly NotifyDbContext _context;

    public NotificationChannelsController(NotifyDbContext context)
    {
        _context = context;
    }

    [HttpPost("seed-default")]
    public async Task<IActionResult> SeedDefaultChannels()
    {
        var channels = new List<NotificationChannel>
        {
            new NotificationChannel
            {
                Id = Guid.NewGuid(),
                Name = "In-App Notification",
                Code = "IN_APP",
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow
            },
            new NotificationChannel
            {
                Id = Guid.NewGuid(),
                Name = "Email Notification",
                Code = "EMAIL",
                IsEnabled = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        foreach (var channel in channels)
        {
            var existing = await _context.NotificationChannels
                .FirstOrDefaultAsync(x => x.Code == channel.Code);

            if (existing == null)
            {
                _context.NotificationChannels.Add(channel);
            }
            else
            {
                existing.Name = channel.Name;
                existing.IsEnabled = channel.IsEnabled;
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Seed default notification channels successfully"
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetChannels()
    {
        var channels = await _context.NotificationChannels
            .OrderBy(x => x.Code)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Code,
                x.IsEnabled,
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(channels);
    }
}