using System.ComponentModel.DataAnnotations;

namespace ProjectService.DTOs.Sprints;

public class SprintDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int DurationDays => (EndDate - StartDate).Days;
    public DateTime CreatedAt { get; set; }
}

public class CreateSprintRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Goal { get; set; }

    [Required]
    public DateTime StartDate { get; set; }
}

public class UpdateSprintRequest
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Goal { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
