namespace NotifyService.Api.Models;

public class CommentAttachment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CommentId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FileUrl { get; set; } = string.Empty;

    public string? ContentType { get; set; }

    public long FileSize { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Comment? Comment { get; set; }
}