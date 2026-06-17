using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotifyService.Api.Data;
using NotifyService.Api.Models;

namespace NotifyService.Api.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly AppDbContext _context;

    public RolesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _context.SystemRoles
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Name,
                x.Description,
                x.IsActive
            })
            .ToListAsync();

        return Ok(roles);
    }

    [HttpPost("seed-default")]
    public async Task<IActionResult> SeedDefaultRoles()
    {
        var roles = new List<SystemRole>
        {
            new SystemRole
            {
                Id = Guid.NewGuid(),
                Code = "Admin",
                Name = "Administrator",
                Description = "Quáº£n trá»‹ há»‡ thá»‘ng",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new SystemRole
            {
                Id = Guid.NewGuid(),
                Code = "User",
                Name = "User",
                Description = "NgÆ°á»i dÃ¹ng há»‡ thá»‘ng",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        foreach (var role in roles)
        {
            var existing = await _context.SystemRoles
                .FirstOrDefaultAsync(x => x.Code == role.Code);

            if (existing == null)
            {
                _context.SystemRoles.Add(role);
            }
            else
            {
                existing.Name = role.Name;
                existing.Description = role.Description;
                existing.IsActive = true;
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Seed system roles successfully" });
    }
}
