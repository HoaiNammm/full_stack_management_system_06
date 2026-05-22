using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotifyService.Api.DTOs;
using NotifyService.Api.Models;
using NotifyService.Api.Services;
using Microsoft.EntityFrameworkCore;
using NotifyService.Api.Data;
namespace NotifyService.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly JwtService _jwtService;

    private readonly NotifyDbContext _context;
    public AuthController(JwtService jwtService, NotifyDbContext context)
    {
        _jwtService = jwtService;
        _context = context;
    }
    // Lo gin API
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var email = request.Email.Trim().ToLower();

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email.ToLower() == email);

            if (user == null)
            {
                return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });
            }

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                _context.LoginHistories.Add(new LoginHistory
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    LoginAt = DateTime.UtcNow,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers.UserAgent.ToString(),
                    IsSuccess = false,
                    FailureReason = "Invalid password"
                });

                await _context.SaveChangesAsync();

                return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });
            }

            if (!user.IsActive)
            {
                _context.LoginHistories.Add(new LoginHistory
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    LoginAt = DateTime.UtcNow,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers.UserAgent.ToString(),
                    IsSuccess = false,
                    FailureReason = "User inactive"
                });

                await _context.SaveChangesAsync();

                return Unauthorized(new { message = "Tài khoản đã bị khóa" });
            }

            _context.LoginHistories.Add(new LoginHistory
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                LoginAt = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString(),
                IsSuccess = true,
                FailureReason = null
            });

            await _context.SaveChangesAsync();


            // Login tạo và lưu RefreshToken
            var token = _jwtService.GenerateToken(user, out var expiresAt);

            var refreshTokenValue = _jwtService.GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshTokenValue,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshToken);

            await _context.SaveChangesAsync();

            return Ok(new LoginResponse
            {
                Token = token,
                RefreshToken = refreshTokenValue,
                ExpiresAt = expiresAt,
                User = new UserInfo
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Lỗi hệ thống khi đăng nhập",
                error = ex.Message
            });
        }
    }
    // Lich su dang nhap 
    [HttpGet("login-histories")]
    public async Task<IActionResult> GetLoginHistories()
    {
        var histories = await _context.LoginHistories
            .OrderByDescending(x => x.LoginAt)
            .Select(x => new
            {
                x.Id,
                x.UserId,
                x.LoginAt,
                x.IpAddress,
                x.UserAgent,
                x.IsSuccess,
                x.FailureReason
            })
            .ToListAsync();

        return Ok(histories);
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var email = request.Email.Trim().ToLower();

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(x => x.Email.ToLower() == email);

        if (existingUser != null)
        {
            return BadRequest(new { message = "Email đã tồn tại" });
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Tạo tài khoản thành công",
            user = new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Role,
                user.IsActive
            }
        });
    }    
        // User nhập email
        // → Backend tạo reset token
        // → Lưu vào bảng PasswordResetTokens
        // → User dùng reset token để đặt mật khẩu mới

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var email = request.Email.Trim().ToLower();

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email.ToLower() == email);

        if (user == null)
        {
            return Ok(new
            {
                message = "Nếu email tồn tại trong hệ thống, reset token sẽ được tạo."
            });
        }

        var resetTokenValue = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "");

        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = resetTokenValue,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.PasswordResetTokens.Add(resetToken);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Reset token đã được tạo. demo",
            resetToken = resetTokenValue,
            expiresAt = resetToken.ExpiresAt
        });
    }


    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var resetToken = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(x =>
                x.Token == request.Token &&
                !x.IsUsed &&
                x.ExpiresAt > DateTime.UtcNow);

        if (resetToken == null)
        {
            return BadRequest(new { message = "Reset token không hợp lệ hoặc đã hết hạn" });
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == resetToken.UserId);

        if (user == null)
        {
            return BadRequest(new { message = "Người dùng không tồn tại" });
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        resetToken.IsUsed = true;
        resetToken.UsedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Đổi mật khẩu thành công"
        });
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