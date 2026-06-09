namespace NotifyService.Api.Models;

public class CommentMention
{
    public Guid Id { get; set; }
    public Guid CommentId { get; set; }
    public Guid UserId { get; set; } // logical ref → NotifyDB.Users

    public Comment Comment { get; set; } = null!;
}
