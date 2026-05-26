namespace NotifyService.Api.DTOs;

public class CommentResponse
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }

    public Guid? ProjectId { get; set; }

    public Guid UserId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public List<Guid> MentionedUserIds { get; set; } = new();
    public List<CommentAttachmentResponse> Attachments { get; set; } = new();   

}