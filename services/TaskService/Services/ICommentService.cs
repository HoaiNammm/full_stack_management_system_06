using TaskService.DTOs.Comments;

namespace TaskService.Services;

public interface ICommentService
{
    Task<List<CommentDto>> GetByTaskAsync(Guid taskId, Guid userId);
    Task<CommentDto> CreateAsync(Guid taskId, CreateCommentRequest request, Guid userId);
    Task<CommentDto> UpdateAsync(Guid id, UpdateCommentRequest request, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
}
