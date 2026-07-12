using TaskService.DTOs.TimeLogs;

namespace TaskService.Services;

public interface ITimeLogService
{
    Task<List<TimeLogDto>> GetByTaskAsync(Guid taskId, Guid userId);
    Task<TimeLogDto> LogAsync(Guid taskId, CreateTimeLogRequest request, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
}
