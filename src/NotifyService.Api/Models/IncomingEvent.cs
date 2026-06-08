namespace NotifyService.Api.Models;

public class IncomingEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string EventId { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public string SourceService { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = string.Empty;

    public string Status { get; set; } = "Pending";

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedAt { get; set; }

    public ICollection<EventProcessingLog> EventProcessingLogs { get; set; } = new List<EventProcessingLog>();
}