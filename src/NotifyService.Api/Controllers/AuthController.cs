using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotifyService.Api.DTOs;
using NotifyService.Api.Services;

namespace NotifyService.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly JwtService _jwtService;
    private readonly UserService _userService;

    public AuthController(JwtService jwtService, UserService userService)
    {
        _jwtService  = jwtService;
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userService.GetByEmailAsync(request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });

        if (!user.IsActive)
            return Unauthorized(new { message = "Tài khoản đã bị khóa" });

        await _userService.UpdateLastLoginAsync(user.Id);

        var token = _jwtService.GenerateToken(user, out var expiresAt);

        return Ok(new LoginResponse
        {
            Token     = token,
            ExpiresAt = expiresAt,
            User      = new UserInfo
            {
                Id       = user.Id,
                FullName = user.FullName,
                Email    = user.Email,
                Role     = user.Role
            }
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")!.Value);

        var user = await _userService.GetByIdAsync(userId);
        if (user == null) return NotFound();

        return Ok(new
        {
            id       = user.Id,
            email    = user.Email,
            fullName = user.FullName,
            role     = user.Role,
            avatar   = user.Avatar
        });
    }
}
