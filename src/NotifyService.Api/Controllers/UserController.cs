using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotifyService.Api.Services;

namespace NotifyService.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    // Dùng cho mention autocomplete, member picker
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllAsync();
        var result = users.Select(u => new
        {
            u.Id,
            u.FullName,
            u.Email,
            u.Avatar,
            u.Role
        });
        return Ok(new { success = true, data = result });
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound(new { success = false, error = new { code = "USER_NOT_FOUND" } });

        return Ok(new { success = true, data = new
        {
            user.Id, user.FullName, user.Email, user.Avatar, user.Role, user.Status, user.CreatedAt
        }});
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var existing = await _userService.GetByEmailAsync(request.Email);
        if (existing != null)
            return Conflict(new { success = false, error = new { code = "EMAIL_TAKEN" } });

        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = await _userService.CreateAsync(request.Email, request.FullName, hash, "Member");

        return Ok(new { success = true, data = new
        {
            user.Id, user.FullName, user.Email, user.Role, user.CreatedAt
        }});
    }
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}
