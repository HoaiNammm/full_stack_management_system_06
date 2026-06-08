namespace NotifyService.Api.Models;

public class SystemLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Level { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? Exception { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}