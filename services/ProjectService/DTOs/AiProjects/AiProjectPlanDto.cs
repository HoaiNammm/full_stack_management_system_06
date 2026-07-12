namespace ProjectService.DTOs.AiProjects;

public class AiProjectPlanDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<AiMilestonePlanDto> Milestones { get; set; } = new();
    public List<AiSprintPlanDto> Sprints { get; set; } = new();
    public List<AiTaskPlanDto> Tasks { get; set; } = new();
}

public class AiMilestonePlanDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
}

public class AiSprintPlanDto
{
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class AiTaskPlanDto
{
    public string Title { get; set; } = string.Empty;
    public int Priority { get; set; } = 1;
    public int? SprintIndex { get; set; }
    public decimal? EstimatedHours { get; set; }
    public string? AssigneeName { get; set; }
    public List<string> Subtasks { get; set; } = new();
}

public class AiPreviewRequest
{
    public string Prompt { get; set; } = string.Empty;
}

public class AiRefineRequest
{
    public AiProjectPlanDto Plan { get; set; } = null!;
    public string Refinement { get; set; } = string.Empty;
}

public class AiConfirmRequest
{
    public AiProjectPlanDto Plan { get; set; } = null!;
}

public class AiConfirmResultDto
{
    public Guid ProjectId { get; set; }
}
