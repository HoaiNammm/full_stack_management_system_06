using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using TaskService.DTOs.Comments;
using TaskService.Events;
using TaskService.HttpClients;
using TaskService.Models;

namespace TaskService.Services;

public class CommentServiceImpl : ICommentService
{
    private readonly TaskDbContext _db;
    private readonly RabbitMqEventPublisher _publisher;
    private readonly WorkspaceServiceClient _workspaceClient;
    private readonly UserServiceClient _userClient;
    private readonly NotifyServiceClient _notifyClient;

    public CommentServiceImpl(TaskDbContext db, RabbitMqEventPublisher publisher,
        WorkspaceServiceClient workspaceClient, UserServiceClient userClient,
        NotifyServiceClient notifyClient)
    {
        _db = db;
        _publisher = publisher;
        _workspaceClient = workspaceClient;
        _userClient = userClient;
        _notifyClient = notifyClient;
    }

    public async Task<List<CommentDto>> GetByTaskAsync(Guid taskId, Guid userId)
    {
        var task = await _db.Tasks.FindAsync(taskId)
            ?? throw new KeyNotFoundException("Task not found.");

        await EnsureProjectAccessAsync(task.ProjectId, userId);

        var comments = await _db.Comments
            .Where(c => c.TaskId == taskId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        var authorIds = comments.Select(c => c.AuthorId).Distinct();
        var users = await _userClient.GetUsersAsync(authorIds);
        var userMap = users.ToDictionary(u => u.Id);

        return comments.Select(c => new CommentDto
        {
            Id = c.Id,
            TaskId = c.TaskId,
            AuthorId = c.AuthorId,
            Content = c.Content,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            Author = userMap.TryGetValue(c.AuthorId, out var u)
                ? new AuthorInfo { Id = u.Id, Name = u.Name, AvatarUrl = u.AvatarUrl }
                : null
        }).ToList();
    }

    public async Task<CommentDto> CreateAsync(Guid taskId, CreateCommentRequest request, Guid userId)
    {
        var task = await _db.Tasks.FindAsync(taskId)
            ?? throw new KeyNotFoundException("Task not found.");

        await EnsureProjectAccessAsync(task.ProjectId, userId);

        var comment = new Comment { TaskId = taskId, AuthorId = userId, Content = request.Content };
        _db.Comments.Add(comment);
        _db.ActivityLogs.Add(new Models.ActivityLog { ProjectId = task.ProjectId, TaskId = taskId, ActorId = userId, Action = "CommentAdded", EntityName = task.Title });
        await _db.SaveChangesAsync();

        await _publisher.PublishAsync("comment.added",
            new CommentAddedEvent(comment.Id, taskId, task.ProjectId, userId, DateTime.UtcNow));

        var author = await _userClient.GetUserAsync(userId);

        // Send mention notifications
        var mentionedIds = request.MentionedUserIds.Where(id => id != userId).Distinct().ToList();
        if (mentionedIds.Any())
        {
            var authorName = author?.Name ?? "Someone";
            await _notifyClient.CreateNotificationAsync(new
            {
                type             = "comment_mention",
                title            = "You were mentioned in a comment",
                message          = $"{authorName} mentioned you: \"{(request.Content.Length > 80 ? request.Content[..80] + "…" : request.Content)}\"",
                recipientIds     = mentionedIds,
                relatedTaskId    = taskId,
                relatedProjectId = task.ProjectId,
            });
        }
        return new CommentDto
        {
            Id = comment.Id,
            TaskId = comment.TaskId,
            AuthorId = comment.AuthorId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            Author = author is null ? null : new AuthorInfo { Id = author.Id, Name = author.Name, AvatarUrl = author.AvatarUrl }
        };
    }

    public async Task<CommentDto> UpdateAsync(Guid id, UpdateCommentRequest request, Guid userId)
    {
        var comment = await _db.Comments.FindAsync(id)
            ?? throw new KeyNotFoundException("Comment not found.");

        if (comment.AuthorId != userId)
            throw new UnauthorizedAccessException("Only the author can edit this comment.");

        comment.Content = request.Content;
        comment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new CommentDto { Id = comment.Id, TaskId = comment.TaskId, AuthorId = comment.AuthorId, Content = comment.Content, CreatedAt = comment.CreatedAt, UpdatedAt = comment.UpdatedAt };
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var comment = await _db.Comments.FindAsync(id)
            ?? throw new KeyNotFoundException("Comment not found.");

        if (comment.AuthorId != userId)
            throw new UnauthorizedAccessException("Only the author can delete this comment.");

        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();
    }

    private async Task EnsureProjectAccessAsync(Guid projectId, Guid userId)
    {
        var isMember = await _workspaceClient.IsProjectMemberAsync(projectId, userId);
        if (!isMember) throw new UnauthorizedAccessException("You are not a member of this project.");
    }
}
