namespace NotifyService.Api.DTOs;

public class IncomingEventResponse
{
    public Guid Id { get; set; }

    public string EventId { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public string SourceService { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime ReceivedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }
}