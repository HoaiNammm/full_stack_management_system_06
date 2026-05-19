using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotifyService.Api.DTOs;
using NotifyService.Api.Models;
using NotifyService.Api.Services;

namespace NotifyService.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly JwtService _jwtService;

    private static readonly List<User> Users = new()
    {
        new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            FullName = "Project Manager Demo",
            Email = "2@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = "ProjectManager"
        },
        new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            FullName = "Member Demo",
            Email = "1@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = "Member"
        },
        new User
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            FullName = "Viewer Demo",
            Email = "3@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = "Viewer"
        }
    };

    public AuthController(JwtService jwtService)
    {
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = Users.FirstOrDefault(x =>
            x.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));

        if (user == null)
        {
            return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });
        }

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!isPasswordValid)
        {
            return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });
        }

        if (!user.IsActive)
        {
            return Unauthorized(new { message = "Tài khoản đã bị khóa" });
        }

        var token = _jwtService.GenerateToken(user, out var expiresAt);

        var response = new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new UserInfo
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            }
        };

        return Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var name = User.FindFirst(ClaimTypes.Name)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new
        {
            id = userId,
            email,
            fullName = name,
            role
        });
    }
}