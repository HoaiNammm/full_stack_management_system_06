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
    private readonly IWebHostEnvironment _environment;

    public AuthController(JwtService jwtService, UserService userService, IWebHostEnvironment environment)
    {
        _jwtService  = jwtService;
        _userService = userService;
        _environment = environment;
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
            avatar   = user.Avatar ?? user.AvatarUrl,
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
}
