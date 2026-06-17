namespace NotifyService.Api.DTOs;

public class CreateProjectDiscussionRequest
{
    public string Content { get; set; } = string.Empty;

    public List<Guid> RecipientUserIds { get; set; } = new();
}