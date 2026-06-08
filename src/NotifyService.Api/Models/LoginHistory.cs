namespace NotifyService.Api.Models;

public class LoginHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public DateTime LoginAt { get; set; } = DateTime.UtcNow;

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public bool IsSuccess { get; set; }

    public string? FailureReason { get; set; }

    public User? User { get; set; }
}