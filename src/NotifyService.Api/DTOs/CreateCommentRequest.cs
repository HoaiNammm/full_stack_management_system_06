namespace NotifyService.Api.DTOs;

public class CreateCommentRequest
{
    public Guid TaskId { get; set; }

    public Guid? ProjectId { get; set; }

    public string Content { get; set; } = string.Empty;
}