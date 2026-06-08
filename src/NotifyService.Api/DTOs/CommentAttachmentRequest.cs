namespace NotifyService.Api.DTOs;

public class CommentAttachmentRequest
{
    public string FileName { get; set; } = string.Empty;

    public string FileUrl { get; set; } = string.Empty;

    public string? ContentType { get; set; }

    public long FileSize { get; set; }
}