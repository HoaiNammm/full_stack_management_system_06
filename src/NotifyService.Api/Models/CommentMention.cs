namespace NotifyService.Api.Models;

public class CommentMention
{
    public Guid Id { get; set; }
    public Guid CommentId { get; set; }
    public Guid UserId { get; set; }
    public Guid MentionedUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Comment Comment { get; set; } = null!;
}
