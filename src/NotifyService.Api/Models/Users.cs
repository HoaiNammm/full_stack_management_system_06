namespace NotifyService.Api.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "Member";

    public string? PhoneNumber { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Department { get; set; }

    public string? Position { get; set; }

    public bool IsActive { get; set; } = true;

    public bool EmailConfirmed { get; set; } = false;

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public ICollection<LoginHistory> LoginHistories { get; set; } = new List<LoginHistory>();

    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
}