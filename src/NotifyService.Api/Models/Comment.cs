namespace NotifyService.Api.Models;

public class Comment
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<CommentMention> Mentions { get; set; } = new List<CommentMention>();
    public ICollection<CommentAttachment> Attachments { get; set; } = new List<CommentAttachment>();
}
