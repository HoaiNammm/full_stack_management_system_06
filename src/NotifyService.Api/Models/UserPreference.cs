namespace NotifyService.Api.Models;

public class UserPreference
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public bool EnableTaskNotification { get; set; } = true;
    public bool EnableCommentNotification { get; set; } = true;
    public bool EnableMentionNotification { get; set; } = true;
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}
