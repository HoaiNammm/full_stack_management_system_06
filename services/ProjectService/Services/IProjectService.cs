using ProjectService.DTOs.Members;
using ProjectService.DTOs.Projects;
using ProjectService.DTOs.Sprints;

namespace ProjectService.Services;

public interface IProjectService
{
    Task<List<ProjectDto>> GetByWorkspaceAsync(Guid workspaceId, Guid userId);
    Task<ProjectDto?> GetByIdAsync(Guid id, Guid userId);
    Task<ProjectDto> CreateAsync(Guid workspaceId, CreateProjectRequest request, Guid userId);
    Task<ProjectDto> UpdateAsync(Guid id, UpdateProjectRequest request, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
    Task<List<ProjectMemberDto>> GetMembersAsync(Guid projectId, Guid userId);
    Task<ProjectMemberDto> AddMemberAsync(Guid projectId, AddProjectMemberRequest request, Guid userId);
    Task<ProjectMemberDto> UpdateMemberRoleAsync(Guid projectId, Guid memberId, UpdateMemberRoleRequest request, Guid userId);
    Task RemoveMemberAsync(Guid projectId, Guid memberId, Guid userId);
    Task<List<Guid>> GetMemberIdsAsync(Guid projectId);

    Task<List<SprintDto>> GetSprintsAsync(Guid projectId, Guid userId);
    Task<SprintDto> CreateSprintAsync(Guid projectId, CreateSprintRequest request, Guid userId);
    Task<SprintDto> UpdateSprintAsync(Guid projectId, Guid sprintId, UpdateSprintRequest request, Guid userId);
    Task<SprintDto> StartSprintAsync(Guid projectId, Guid sprintId, Guid userId);
    Task<SprintDto> CompleteSprintAsync(Guid projectId, Guid sprintId, Guid userId);
    Task DeleteSprintAsync(Guid projectId, Guid sprintId, Guid userId);
}
