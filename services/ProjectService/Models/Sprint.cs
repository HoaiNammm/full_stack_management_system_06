namespace ProjectService.Models;

public class Sprint
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SprintStatus Status { get; set; } = SprintStatus.Planning;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Project Project { get; set; } = null!;

    public static Sprint CreateTwoWeek(Guid projectId, string name, string? goal, DateTime startDate) =>
        new()
        {
            ProjectId = projectId,
            Name = name,
            Goal = goal,
            StartDate = startDate,
            EndDate = startDate.AddDays(14)
        };
}

public enum SprintStatus { Planning, Active, Completed }
