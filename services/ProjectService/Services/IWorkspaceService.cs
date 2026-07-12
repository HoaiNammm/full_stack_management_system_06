using ProjectService.DTOs.Members;
using ProjectService.DTOs.Workspaces;

namespace ProjectService.Services;

public interface IWorkspaceService
{
    Task<List<WorkspaceDto>> GetAllByUserAsync(Guid userId);
    Task<WorkspaceDto?> GetByIdAsync(Guid id, Guid userId);
    Task<WorkspaceDto> CreateAsync(CreateWorkspaceRequest request, Guid ownerId);
    Task<WorkspaceDto> UpdateAsync(Guid id, UpdateWorkspaceRequest request, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
    Task<List<WorkspaceMemberDto>> GetMembersAsync(Guid workspaceId, Guid requesterId);
    Task<WorkspaceMemberDto> AddMemberAsync(Guid workspaceId, AddWorkspaceMemberRequest request, Guid requesterId);
    Task<WorkspaceMemberDto> UpdateMemberRoleAsync(Guid workspaceId, Guid memberId, UpdateMemberRoleRequest request, Guid requesterId);
    Task RemoveMemberAsync(Guid workspaceId, Guid memberId, Guid requesterId);
}
