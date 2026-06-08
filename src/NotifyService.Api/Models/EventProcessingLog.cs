namespace NotifyService.Api.Models;

public class EventProcessingLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid IncomingEventId { get; set; }

    public string Step { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? Message { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public IncomingEvent? IncomingEvent { get; set; }
}