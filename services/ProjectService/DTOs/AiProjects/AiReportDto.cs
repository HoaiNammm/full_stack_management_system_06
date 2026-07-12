namespace ProjectService.DTOs.AiProjects;

public class AiReportRequest
{
    public string Question { get; set; } = string.Empty;
}

public class AiReportResponseDto
{
    public string Report { get; set; } = string.Empty;
}
