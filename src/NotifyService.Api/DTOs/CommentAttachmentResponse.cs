namespace NotifyService.Api.DTOs;

public class CommentAttachmentResponse
{
    public Guid Id { get; set; }

    public Guid CommentId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FileUrl { get; set; } = string.Empty;

    public string? ContentType { get; set; }

    public long FileSize { get; set; }

    public DateTime CreatedAt { get; set; }
}