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
    private readonly NotifyDbContext _context;

    public NotificationTemplatesController(NotifyDbContext context)
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
                TitleTemplate = "Bạn được nhắc đến trong một bình luận",
                MessageTemplate = "Có người đã nhắc đến bạn trong một bình luận của task.",
                Type = "COMMENT_MENTION",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new NotificationTemplate
            {
                Id = Guid.NewGuid(),
                Code = "COMMENT_CREATED",
                TitleTemplate = "Có bình luận mới",
                MessageTemplate = "Một bình luận mới đã được tạo trong task.",
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