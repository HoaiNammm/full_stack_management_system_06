namespace NotifyService.Api.DTOs;

public class CreateUserRequest
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = "Member";

    public string? PhoneNumber { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Department { get; set; }

    public string? Position { get; set; }
}