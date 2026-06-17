using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotifyService.Api.Data;
using NotifyService.Api.DTOs;
using NotifyService.Api.Models;

namespace NotifyService.Api.Controllers;

[ApiController]
[Route("api/admin-users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.FullName,
                x.Email,
                x.Role,
                x.PhoneNumber,
                x.AvatarUrl,
                x.Department,
                x.Position,
                x.IsActive,
                x.EmailConfirmed,
                x.LastLoginAt,
                x.CreatedAt,
                x.UpdatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _context.Users
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.FullName,
                x.Email,
                x.Role,
                x.PhoneNumber,
                x.AvatarUrl,
                x.Department,
                x.Position,
                x.IsActive,
                x.EmailConfirmed,
                x.LastLoginAt,
                x.CreatedAt,
                x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound(new { message = "KhÃ´ng tÃ¬m tháº¥y ngÆ°á»i dÃ¹ng" });
        }

        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var email = request.Email.Trim().ToLower();

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(x => x.Email.ToLower() == email);

        if (existingUser != null)
        {
            return BadRequest(new { message = "Email Ä‘Ã£ tá»“n táº¡i" });
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest(new { message = "Há» tÃªn khÃ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng" });
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Máº­t kháº©u khÃ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng" });
        }
        var role = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role;

        var roleExists = await _context.SystemRoles
            .AnyAsync(x => x.Code == role && x.IsActive);

        if (!roleExists)
        {
            return BadRequest(new { message = "Role khÃ´ng há»£p lá»‡ hoáº·c chÆ°a Ä‘Æ°á»£c kÃ­ch hoáº¡t" });
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = role,
            PhoneNumber = request.PhoneNumber,
            AvatarUrl = request.AvatarUrl,
            Department = request.Department,
            Position = request.Position,
            IsActive = true,
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };



        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Táº¡o ngÆ°á»i dÃ¹ng thÃ nh cÃ´ng",
            user = new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Role,
                user.PhoneNumber,
                user.AvatarUrl,
                user.Department,
                user.Position,
                user.IsActive,
                user.EmailConfirmed,
                user.CreatedAt
            }
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
        {
            return NotFound(new { message = "KhÃ´ng tÃ¬m tháº¥y ngÆ°á»i dÃ¹ng" });
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest(new { message = "Há» tÃªn khÃ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng" });
        }

        user.FullName = request.FullName.Trim();
        user.PhoneNumber = request.PhoneNumber;
        user.AvatarUrl = request.AvatarUrl;
        user.Department = request.Department;
        user.Position = request.Position;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Cáº­p nháº­t ngÆ°á»i dÃ¹ng thÃ nh cÃ´ng",
            user = new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Role,
                user.PhoneNumber,
                user.AvatarUrl,
                user.Department,
                user.Position,
                user.IsActive,
                user.UpdatedAt
            }
        });
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
        {
            return NotFound(new { message = "KhÃ´ng tÃ¬m tháº¥y ngÆ°á»i dÃ¹ng" });
        }

        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = request.IsActive ? "ÄÃ£ kÃ­ch hoáº¡t tÃ i khoáº£n" : "ÄÃ£ khÃ³a tÃ i khoáº£n",
            user.Id,
            user.Email,
            user.IsActive
        });
    }

    [HttpPatch("{id:guid}/role")]
    public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleRequest request)
    {
        var roleExists = await _context.SystemRoles.AnyAsync(x => x.Code == request.Role && x.IsActive);

        if (!roleExists)
        {
            return BadRequest(new
            {
                message = "Role khÃ´ng há»£p lá»‡ hoáº·c chÆ°a Ä‘Æ°á»£c kÃ­ch hoáº¡t"
            });
        }

        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
        {
            return NotFound(new { message = "KhÃ´ng tÃ¬m tháº¥y ngÆ°á»i dÃ¹ng" });
        }

        user.Role = request.Role;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Cáº­p nháº­t quyá»n ngÆ°á»i dÃ¹ng thÃ nh cÃ´ng",
            user.Id,
            user.Email,
            user.Role
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
        {
            return NotFound(new { message = "KhÃ´ng tÃ¬m tháº¥y ngÆ°á»i dÃ¹ng" });
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "ÄÃ£ khÃ³a tÃ i khoáº£n ngÆ°á»i dÃ¹ng",
            user.Id,
            user.Email,
            user.IsActive
        });
    }
}

