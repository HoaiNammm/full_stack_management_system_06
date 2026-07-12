using System.ComponentModel.DataAnnotations;

namespace TaskService.DTOs.TimeLogs;

public class TimeLogDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public string? Description { get; set; }
    public decimal HoursLogged { get; set; }
    public DateTime LoggedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTimeLogRequest
{
    [Required]
    [Range(0.25, 24.0)]
    public decimal HoursLogged { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime? LoggedAt { get; set; }
}
