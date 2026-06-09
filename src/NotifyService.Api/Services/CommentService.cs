using Microsoft.EntityFrameworkCore;
using NotifyService.Api.Data;
using NotifyService.Api.Models;

namespace NotifyService.Api.Services;

public class CommentService
{
    private readonly AppDbContext _context;
    private readonly NotificationService _notificationService;

    public CommentService(AppDbContext context, NotificationService notificationService)
    {
        _context             = context;
        _notificationService = notificationService;
    }

    public async Task<List<Comment>> GetByTaskAsync(Guid taskId) =>
        await _context.Comments
            .Include(c => c.Author)
            .Include(c => c.Mentions)
            .Where(c => c.TaskId == taskId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

    public async Task<Comment> CreateAsync(Guid taskId, Guid authorId, string content,
        List<Guid> mentionedUserIds, Guid? taskAssigneeId)
    {
        var comment = new Comment
        {
            Id        = Guid.NewGuid(),
            TaskId    = taskId,
            AuthorId  = authorId,
            Content   = content,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);

        foreach (var userId in mentionedUserIds.Distinct())
        {
            _context.CommentMentions.Add(new CommentMention
            {
                Id        = Guid.NewGuid(),
                CommentId = comment.Id,
                UserId    = userId
            });
        }

        await _context.SaveChangesAsync();

        // Notify task assignee (comment.created) — bỏ qua nếu assignee chính là author
        if (taskAssigneeId.HasValue && taskAssigneeId != authorId)
        {
            await _notificationService.CreateAsync(
                userId:        taskAssigneeId.Value,
                title:         "Bình luận mới trên task của bạn",
                content:       $"Có bình luận mới: \"{TruncateContent(content)}\"",
                type:          "comment_created",
                relatedTaskId: taskId);
        }

        // Notify từng mentioned user (user.mentioned) — bỏ qua author
        foreach (var userId in mentionedUserIds.Distinct().Where(id => id != authorId))
        {
            await _notificationService.CreateAsync(
                userId:        userId,
                title:         "Bạn được nhắc đến trong bình luận",
                content:       $"\"{TruncateContent(content)}\"",
                type:          "comment_mention",
                relatedTaskId: taskId);
        }

        return comment;
    }

    public async Task<Comment?> UpdateAsync(Guid commentId, Guid authorId, string content)
    {
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == commentId && c.AuthorId == authorId);
        if (comment == null) return null;

        comment.Content   = content;
        comment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task<bool> DeleteAsync(Guid commentId, Guid authorId)
    {
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == commentId && c.AuthorId == authorId);
        if (comment == null) return false;

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
        return true;
    }

    private static string TruncateContent(string content) =>
        content.Length > 80 ? content[..80] + "…" : content;
}
