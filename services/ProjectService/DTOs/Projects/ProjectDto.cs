using System.ComponentModel.DataAnnotations;

namespace ProjectService.DTOs.Projects;

public class ProjectDto
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = "#3B82F6";
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? TeamLeadId { get; set; }
    public int Progress { get; set; }
    public int MemberCount { get; set; }
    public int SprintCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateProjectRequest
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(20)]
    public string Color { get; set; } = "#3B82F6";

    public string Priority { get; set; } = "Medium";
    public string Status { get; set; } = "Planning";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? TeamLeadId { get; set; }
}

public class UpdateProjectRequest
{
    [MaxLength(150)]
    public string? Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(20)]
    public string? Color { get; set; }

    public string? Priority { get; set; }
    public string? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? TeamLeadId { get; set; }
    public int? Progress { get; set; }
}
