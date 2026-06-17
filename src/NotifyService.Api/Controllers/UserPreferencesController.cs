using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotifyService.Api.Data;
using NotifyService.Api.DTOs;
using NotifyService.Api.Models;

namespace NotifyService.Api.Controllers;

[ApiController]
[Route("api/user-preferences")]
[Authorize]
public class UserPreferencesController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserPreferencesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyPreferences()
    {
        var userId = GetCurrentUserId();

        var preference = await _context.UserPreferences
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (preference == null)
        {
            preference = new UserPreference
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EnableTaskNotification = true,
                EnableCommentNotification = true,
                EnableMentionNotification = true,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserPreferences.Add(preference);
            await _context.SaveChangesAsync();
        }

        return Ok(new
        {
            preference.Id,
            preference.UserId,
            preference.EnableTaskNotification,
            preference.EnableCommentNotification,
            preference.EnableMentionNotification,
            preference.UpdatedAt
        });
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyPreferences(
        [FromBody] UpdateUserPreferenceRequest request)
    {
        var userId = GetCurrentUserId();

        var preference = await _context.UserPreferences
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (preference == null)
        {
            preference = new UserPreference
            {
                Id = Guid.NewGuid(),
                UserId = userId
            };

            _context.UserPreferences.Add(preference);
        }

        preference.EnableTaskNotification =
            request.EnableTaskNotification;

        preference.EnableCommentNotification =
            request.EnableCommentNotification;

        preference.EnableMentionNotification =
            request.EnableMentionNotification;

        preference.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Cập nhật tùy chọn thông báo thành công",
            preference.EnableTaskNotification,
            preference.EnableCommentNotification,
            preference.EnableMentionNotification,
            preference.UpdatedAt
        });
    }

    private Guid GetCurrentUserId()
    {
        var value =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException(
                "Token không chứa UserId hợp lệ");
        }

        return userId;
    }
}