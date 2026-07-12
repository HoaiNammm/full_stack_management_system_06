using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotifyService.DTOs.Auth;
using NotifyService.DTOs.Users;
using NotifyService.Services;

namespace NotifyService.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (currentUserId != id) return Forbid();

        var user = await _userService.UpdateAsync(id, request);
        return Ok(user);
    }

    [HttpPut("{id:guid}/change-password")]
    public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordRequest request)
    {
        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (currentUserId != id) return Forbid();

        await _userService.ChangePasswordAsync(id, request);
        return NoContent();
    }
}

[ApiController]
[Route("internal/users")]
public class UsersInternalController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersInternalController(IUserService userService) => _userService = userService;

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost("batch")]
    public async Task<IActionResult> GetBatch([FromBody] List<Guid> ids)
    {
        var users = await _userService.GetByIdsAsync(ids);
        return Ok(users);
    }

    [HttpGet("by-email")]
    public async Task<IActionResult> GetByEmail([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return BadRequest("email is required");
        var user = await _userService.GetByEmailAsync(email);
        return user is null ? NotFound() : Ok(user);
    }
}
