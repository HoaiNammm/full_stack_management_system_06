using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotifyService.Api.Data;
using NotifyService.Api.Models;

namespace NotifyService.Api.Controllers;

[ApiController]
[Route("api/notification-templates")]
[Authorize]
public class NotificationTemplatesController : ControllerBase
{
    private readonly AppDbContext _context;

    public NotificationTemplatesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("seed-default")]
    public async Task<IActionResult> SeedDefaultTemplates()
    {
        var templates = new List<NotificationTemplate>
        {
            new NotificationTemplate
            {
                Id = Guid.NewGuid(),
                Code = "COMMENT_MENTION",
                TitleTemplate = "Báº¡n Ä‘Æ°á»£c nháº¯c Ä‘áº¿n trong má»™t bÃ¬nh luáº­n",
                MessageTemplate = "CÃ³ ngÆ°á»i Ä‘Ã£ nháº¯c Ä‘áº¿n báº¡n trong má»™t bÃ¬nh luáº­n cá»§a task.",
                Type = "COMMENT_MENTION",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new NotificationTemplate
            {
                Id = Guid.NewGuid(),
                Code = "COMMENT_CREATED",
                TitleTemplate = "CÃ³ bÃ¬nh luáº­n má»›i",
                MessageTemplate = "Má»™t bÃ¬nh luáº­n má»›i Ä‘Ã£ Ä‘Æ°á»£c táº¡o trong task.",
                Type = "COMMENT_CREATED",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        foreach (var template in templates)
        {
            var existing = await _context.NotificationTemplates
                .FirstOrDefaultAsync(x => x.Code == template.Code);

            if (existing == null)
            {
                _context.NotificationTemplates.Add(template);
            }
            else
            {
                existing.TitleTemplate = template.TitleTemplate;
                existing.MessageTemplate = template.MessageTemplate;
                existing.Type = template.Type;
                existing.IsActive = true;
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Seed default notification templates successfully"
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetTemplates()
    {
        var templates = await _context.NotificationTemplates
            .OrderBy(x => x.Code)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.TitleTemplate,
                x.MessageTemplate,
                x.Type,
                x.IsActive,
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(templates);
    }
}
