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
    private readonly NotifyDbContext _context;

    public UsersController(NotifyDbContext context)
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
            return NotFound(new { message = "Không tìm thấy người dùng" });
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
            return BadRequest(new { message = "Email đã tồn tại" });
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest(new { message = "Họ tên không được để trống" });
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Mật khẩu không được để trống" });
        }
        var role = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role;

        var roleExists = await _context.SystemRoles
            .AnyAsync(x => x.Code == role && x.IsActive);

        if (!roleExists)
        {
            return BadRequest(new { message = "Role không hợp lệ hoặc chưa được kích hoạt" });
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
            message = "Tạo người dùng thành công",
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
            return NotFound(new { message = "Không tìm thấy người dùng" });
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest(new { message = "Họ tên không được để trống" });
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
            message = "Cập nhật người dùng thành công",
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
            return NotFound(new { message = "Không tìm thấy người dùng" });
        }

        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = request.IsActive ? "Đã kích hoạt tài khoản" : "Đã khóa tài khoản",
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
                message = "Role không hợp lệ hoặc chưa được kích hoạt"
            });
        }

        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
        {
            return NotFound(new { message = "Không tìm thấy người dùng" });
        }

        user.Role = request.Role;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Cập nhật quyền người dùng thành công",
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
            return NotFound(new { message = "Không tìm thấy người dùng" });
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Đã khóa tài khoản người dùng",
            user.Id,
            user.Email,
            user.IsActive
        });
    }
}
