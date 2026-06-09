namespace NotifyService.Api.Models;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string Role { get; set; } = "Member";
    public int Status { get; set; } = 1; // Active=1, Inactive=0
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }

    public bool IsActive => Status == 1;

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    public UserPreference? UserPreference { get; set; }
}
