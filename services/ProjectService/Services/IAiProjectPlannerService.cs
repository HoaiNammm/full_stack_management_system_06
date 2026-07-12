using ProjectService.DTOs.AiProjects;

namespace ProjectService.Services;

public record AiTeamMemberInfo(string Name, string Role);

public interface IAiProjectPlannerService
{
    Task<AiProjectPlanDto> GeneratePlanAsync(string prompt, List<AiTeamMemberInfo> teamMembers);
    Task<AiProjectPlanDto> RefinePlanAsync(AiProjectPlanDto currentPlan, string refinement, List<AiTeamMemberInfo> teamMembers);
    Task<string> AnswerReportQuestionAsync(string projectContext, string question);
}
