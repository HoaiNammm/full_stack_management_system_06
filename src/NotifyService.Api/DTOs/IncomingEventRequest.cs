namespace NotifyService.Api.DTOs;

public class IncomingEventRequest
{
    public string EventId { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public string SourceService { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = string.Empty;
}