using TaskService.DTOs.Tasks;

namespace TaskService.Services;

public interface ITaskService
{
    Task<List<TaskDto>> GetByProjectAsync(Guid projectId, Guid userId, string? status, string? priority);
    Task<TaskDto?> GetByIdAsync(Guid id, Guid userId);
    Task<TaskDto> CreateAsync(Guid projectId, CreateTaskRequest request, Guid userId);
    Task<TaskDto> UpdateAsync(Guid id, UpdateTaskRequest request, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
    Task<TaskStatsDto> GetStatsByProjectAsync(Guid projectId);
    Task<List<TaskDto>> GetAssignedToMeAsync(Guid userId);
}
