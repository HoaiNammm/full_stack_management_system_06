using TaskService.DTOs.SubTasks;

namespace TaskService.Services;

public interface ISubTaskService
{
    Task<List<SubTaskDto>> GetByTaskAsync(Guid taskId, Guid userId);
    Task<SubTaskDto> CreateAsync(Guid taskId, CreateSubTaskRequest request, Guid userId);
    Task<SubTaskDto> UpdateAsync(Guid id, UpdateSubTaskRequest request, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
}
