namespace NotifyService.Api.Models;

public class Comment
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; } // logical ref → TaskDB.Tasks
    public Guid AuthorId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public User Author { get; set; } = null!;
    public ICollection<CommentMention> Mentions { get; set; } = new List<CommentMention>();
}
