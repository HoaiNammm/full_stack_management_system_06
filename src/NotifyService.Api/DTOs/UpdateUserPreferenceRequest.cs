namespace NotifyService.Api.DTOs;

public class UpdateUserPreferenceRequest
{
    public bool EnableTaskNotification { get; set; } = true;

    public bool EnableCommentNotification { get; set; } = true;

    public bool EnableMentionNotification { get; set; } = true;
}