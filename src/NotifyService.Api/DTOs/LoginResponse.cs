namespace NotifyService.Api.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    

    public string RefreshToken { get; set; } = string.Empty;

    public string TokenType { get; set; } = "Bearer";

    public DateTime ExpiresAt { get; set; }

    public UserInfo User { get; set; } = new();
}

public class UserInfo
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Department { get; set; }

    public string? Position { get; set; }

    public bool IsActive { get; set; }

    public bool EmailConfirmed { get; set; }
}