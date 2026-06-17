using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotifyService.Api.DTOs;
using NotifyService.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NotifyService.Api.Configurations;
using NotifyService.Api.Data;
using NotifyService.Api.Models;
using System.Security.Cryptography;
namespace NotifyService.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly JwtService _jwtService;
    private readonly UserService _userService;
    private readonly IWebHostEnvironment _environment;
    private readonly AppDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthController(
    JwtService jwtService,
    UserService userService,
    IWebHostEnvironment environment,
    AppDbContext context,
    IOptions<JwtSettings> jwtOptions)
    {
        _jwtService = jwtService;
        _userService = userService;
        _environment = environment;
        _context = context;
        _jwtSettings = jwtOptions.Value;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new
            {
                message = "Email và mật khẩu không được để trống"
            });
        }

        var user = await _userService.GetByEmailAsync(request.Email.Trim());

        if (user == null)
        {
            // Không lưu được vì LoginHistory.UserId hiện không cho phép null.
            return Unauthorized(new
            {
                message = "Email hoặc mật khẩu không đúng"
            });
        }

        var passwordCorrect = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash);

        if (!passwordCorrect)
        {
            await SaveLoginHistoryAsync(
                user.Id,
                false,
                "Mật khẩu không chính xác");

            return Unauthorized(new
            {
                message = "Email hoặc mật khẩu không đúng"
            });
        }

        if (!user.IsActive)
        {
            await SaveLoginHistoryAsync(
                user.Id,
                false,
                "Tài khoản đã bị khóa");

            return Unauthorized(new
            {
                message = "Tài khoản đã bị khóa"
            });
        }

        await _userService.UpdateLastLoginAsync(user.Id);

        await SaveLoginHistoryAsync(
            user.Id,
            true,
            null);

        var token = _jwtService.GenerateToken(
            user,
            out var expiresAt);

        return Ok(new LoginResponse
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
            id = user.Id,
            email = user.Email,
            fullName = user.FullName,
            role = user.Role,
            avatar = user.Avatar ?? user.AvatarUrl,
            phoneNumber = user.PhoneNumber,
            avatarUrl = user.AvatarUrl,
            department = user.Department,
            position = user.Position
        });
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")!.Value);

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest(new { message = "Họ tên không được để trống" });
        }

        var user = await _userService.UpdateProfileAsync(
            userId,
            request.FullName,
            request.PhoneNumber,
            request.AvatarUrl,
            request.Department,
            request.Position);

        if (user == null) return NotFound(new { message = "Không tìm thấy người dùng" });

        return Ok(new
        {
            id = user.Id,
            email = user.Email,
            fullName = user.FullName,
            role = user.Role,
            avatar = user.Avatar ?? user.AvatarUrl,
            phoneNumber = user.PhoneNumber,
            avatarUrl = user.AvatarUrl,
            department = user.Department,
            position = user.Position
        });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")!.Value);

        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
        {
            return BadRequest(new { message = "Vui lòng nhập mật khẩu hiện tại" });
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
        {
            return BadRequest(new { message = "Mật khẩu mới phải có ít nhất 6 ký tự" });
        }

        var changed = await _userService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
        if (changed == null) return NotFound(new { message = "Không tìm thấy người dùng" });
        if (changed == false) return BadRequest(new { message = "Mật khẩu hiện tại không đúng" });

        return Ok(new { message = "Đổi mật khẩu thành công" });
    }

    [Authorize]
    [HttpPost("avatar")]
    [RequestSizeLimit(2 * 1024 * 1024)]
    public async Task<IActionResult> UploadAvatar(IFormFile? file)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")!.Value);

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Vui long chon anh dai dien" });
        }

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            return BadRequest(new { message = "Chi ho tro anh JPG, PNG, WebP hoac GIF" });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = "Dinh dang anh khong hop le" });
        }

        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var uploadsFolder = Path.Combine(webRoot, "uploads", "avatars");
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{userId:N}-{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using (var stream = System.IO.File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }

        var avatarUrl = $"/uploads/avatars/{fileName}";
        var user = await _userService.UpdateAvatarAsync(userId, avatarUrl);
        if (user == null) return NotFound(new { message = "Khong tim thay nguoi dung" });

        return Ok(new
        {
            id = user.Id,
            email = user.Email,
            fullName = user.FullName,
            role = user.Role,
            avatar = user.Avatar ?? user.AvatarUrl,
            phoneNumber = user.PhoneNumber,
            avatarUrl = user.AvatarUrl,
            department = user.Department,
            position = user.Position
        });
    }
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken(
    [FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new
            {
                message = "Refresh token không được để trống"
            });
        }

        var tokenHash = _jwtService.HashToken(request.RefreshToken);

        var storedToken = await _context.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x =>
                x.Token == tokenHash &&
                !x.IsRevoked &&
                x.ExpiresAt > DateTime.UtcNow);

        if (storedToken?.User == null || !storedToken.User.IsActive)
        {
            return Unauthorized(new
            {
                message = "Refresh token không hợp lệ hoặc đã hết hạn"
            });
        }

        // Rotation: token cũ chỉ được dùng một lần
        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;

        var newRawRefreshToken = _jwtService.GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = storedToken.UserId,
            Token = _jwtService.HashToken(newRawRefreshToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(
                _jwtSettings.RefreshTokenExpirationDays),
            IsRevoked = false
        });

        var accessToken = _jwtService.GenerateToken(
            storedToken.User,
            out var expiresAt);

        await _context.SaveChangesAsync();

        return Ok(new LoginResponse
        {
            Token = accessToken,
            RefreshToken = newRawRefreshToken,
            ExpiresAt = expiresAt,
            User = new UserInfo
            {
                Id = storedToken.User.Id,
                FullName = storedToken.User.FullName,
                Email = storedToken.User.Email,
                Role = storedToken.User.Role,
                PhoneNumber = storedToken.User.PhoneNumber,
                AvatarUrl = storedToken.User.AvatarUrl,
                Department = storedToken.User.Department,
                Position = storedToken.User.Position,
                IsActive = storedToken.User.IsActive,
                EmailConfirmed = storedToken.User.EmailConfirmed
            }
        });
    }
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
    [FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Ok(new
            {
                message = "Đăng xuất thành công"
            });
        }

        var tokenHash = _jwtService.HashToken(request.RefreshToken);

        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(x =>
                x.Token == tokenHash &&
                !x.IsRevoked);

        if (storedToken != null)
        {
            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        return Ok(new
        {
            message = "Đăng xuất thành công"
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
    [FromBody] ForgotPasswordRequest request)
    {
        const string message =
            "Nếu email tồn tại, hệ thống đã tạo yêu cầu đặt lại mật khẩu.";

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new
            {
                message = "Email không được để trống"
            });
        }

        var user = await _userService.GetByEmailAsync(request.Email.Trim());

        // Không thông báo email có tồn tại hay không
        if (user == null)
        {
            return Ok(new { message });
        }

        var oldTokens = await _context.PasswordResetTokens
            .Where(x =>
                x.UserId == user.Id &&
                !x.IsUsed &&
                x.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var oldToken in oldTokens)
        {
            oldToken.IsUsed = true;
            oldToken.UsedAt = DateTime.UtcNow;
        }

        var rawResetToken =
            Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

        _context.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            Token = _jwtService.HashToken(rawResetToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(
                _jwtSettings.PasswordResetExpirationMinutes),
            IsUsed = false
        });

        await _context.SaveChangesAsync();

        // Chỉ trả token trực tiếp khi đang phát triển.
        // Sau này production phải gửi token qua email.
        if (_environment.IsDevelopment())
        {
            return Ok(new
            {
                message,
                resetToken = rawResetToken,
                expiresInMinutes =
                    _jwtSettings.PasswordResetExpirationMinutes
            });
        }

        return Ok(new { message });
    }
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return BadRequest(new
            {
                message = "Token không được để trống"
            });
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword) ||
            request.NewPassword.Length < 6)
        {
            return BadRequest(new
            {
                message = "Mật khẩu mới phải có ít nhất 6 ký tự"
            });
        }

        var tokenHash = _jwtService.HashToken(request.Token);

        var resetToken = await _context.PasswordResetTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x =>
                x.Token == tokenHash &&
                !x.IsUsed &&
                x.ExpiresAt > DateTime.UtcNow);

        if (resetToken?.User == null)
        {
            return BadRequest(new
            {
                message = "Token không hợp lệ hoặc đã hết hạn"
            });
        }

        resetToken.User.PasswordHash =
            BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        resetToken.User.UpdatedAt = DateTime.UtcNow;

        resetToken.IsUsed = true;
        resetToken.UsedAt = DateTime.UtcNow;

        // Sau khi đổi mật khẩu, hủy toàn bộ refresh token cũ
        var refreshTokens = await _context.RefreshTokens
            .Where(x =>
                x.UserId == resetToken.UserId &&
                !x.IsRevoked)
            .ToListAsync();

        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Đặt lại mật khẩu thành công"
        });
    }

    private async Task SaveLoginHistoryAsync(
    Guid userId,
    bool isSuccess,
    string? failureReason)
{
    var loginHistory = new LoginHistory
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        LoginAt = DateTime.UtcNow,
        IpAddress = HttpContext.Connection
            .RemoteIpAddress?
            .ToString(),
        UserAgent = Request.Headers.UserAgent.ToString(),
        IsSuccess = isSuccess,
        FailureReason = failureReason
    };

    _context.LoginHistories.Add(loginHistory);
    await _context.SaveChangesAsync();
}
}
