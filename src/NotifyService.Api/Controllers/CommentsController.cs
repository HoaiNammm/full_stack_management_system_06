using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotifyService.Api.Data;
using NotifyService.Api.DTOs;
using NotifyService.Api.Models;

namespace NotifyService.Api.Controllers;

[ApiController]
[Route("api/comment-workflows")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CommentsController(AppDbContext context)
    {
        _context = context;
    }
    //  CreateComment
    // Kiá»ƒm tra dá»¯ liá»‡u Ä‘áº§u vÃ o  TaskId rá»—ng  & content trá»‘ng -> tráº£ vá» lá»—i 400
    [HttpPost]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request)
    {
        if (request.TaskId == Guid.Empty)
        {
            return BadRequest(new { message = "TaskId khÃ´ng há»£p lá»‡" });
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest(new { message = "Ná»™i dung bÃ¬nh luáº­n khÃ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng" });
        }
        //  TrÃ­ch xuáº¥t user Ä‘Äƒng nháº­p
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Token khÃ´ng há»£p lá»‡ hoáº·c thiáº¿u UserId" });
        }
        // Khá»Ÿi táº¡o Object coment má»›i , gÃ¡n id, gÃ¡n hÆ°á»i gian táº¡o

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            TaskId = request.TaskId,
            ProjectId = request.ProjectId,
            UserId = userId,
            Content = request.Content.Trim(),
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        var mentionedUserIds = request.MentionedUserIds
            .Where(x => x != Guid.Empty && x != userId)
            .Distinct()
            .ToList();
        //  Xu li mentionedUserIds
        foreach (var mentionedUserId in mentionedUserIds)
        {
            comment.Mentions.Add(new CommentMention
            {
                Id = Guid.NewGuid(),
                CommentId = comment.Id,
                MentionedUserId = mentionedUserId,
                CreatedAt = DateTime.UtcNow
            });
        }
        // Xu ly actachment
        foreach (var attachment in request.Attachments)
        {
            if (string.IsNullOrWhiteSpace(attachment.FileName) ||
                string.IsNullOrWhiteSpace(attachment.FileUrl))
            {
                continue;
            }

            comment.Attachments.Add(new CommentAttachment
            {
                Id = Guid.NewGuid(),
                CommentId = comment.Id,
                FileName = attachment.FileName.Trim(),
                FileUrl = attachment.FileUrl.Trim(),
                ContentType = attachment.ContentType,
                FileSize = attachment.FileSize,
                CreatedAt = DateTime.UtcNow
            });
        }

        _context.Comments.Add(comment);
        AddActivityLog(
            userId: userId,
            taskId: comment.TaskId,
            projectId: comment.ProjectId,
            action: "COMMENT_CREATED",
            description: "User created a comment on task",
            metadataJson: $"{{\"commentId\":\"{comment.Id}\"}}"

        );
        AddAuditLog(
            userId: userId,
            action: "CREATE",
            entityName: "Comment",
            entityId: comment.Id.ToString(),
            oldValuesJson: null,
            newValuesJson: $"{{\"content\":\"{comment.Content}\"}}"
        );

        // goi helper khi tao comment

        CreateMentionNotifications(
            comment: comment,
            actorUserId: userId,
            mentionedUserIds: mentionedUserIds
        );
        // LÆ°u vÃ o database
        await _context.SaveChangesAsync();

        var response = new CommentResponse
        {
            Id = comment.Id,
            TaskId = comment.TaskId,
            ProjectId = comment.ProjectId,
            UserId = comment.UserId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            MentionedUserIds = mentionedUserIds,
            Attachments = comment.Attachments.Select(a => new CommentAttachmentResponse
            {
                Id = a.Id,
                CommentId = a.CommentId,
                FileName = a.FileName,
                FileUrl = a.FileUrl,
                ContentType = a.ContentType,
                FileSize = a.FileSize,
                CreatedAt = a.CreatedAt
            }).ToList()
        };

        return Ok(response);
    }
    // Láº¥y danh sÃ¡ch comment
    [HttpGet("task/{taskId:guid}")]
    public async Task<IActionResult> GetCommentsByTask(Guid taskId)
    {
        var comments = await _context.Comments
    .Include(x => x.Mentions)
    .Include(x => x.Attachments)
    .Where(x => x.TaskId == taskId && !x.IsDeleted)
    .OrderBy(x => x.CreatedAt)
    .Select(x => new CommentResponse
    {
        Id = x.Id,
        TaskId = x.TaskId,
        ProjectId = x.ProjectId,
        UserId = x.UserId,
        Content = x.Content,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
        MentionedUserIds = x.Mentions
            .Select(m => m.MentionedUserId)
            .ToList(),
        Attachments = x.Attachments
            .Select(a => new CommentAttachmentResponse
            {
                Id = a.Id,
                CommentId = a.CommentId,
                FileName = a.FileName,
                FileUrl = a.FileUrl,
                ContentType = a.ContentType,
                FileSize = a.FileSize,
                CreatedAt = a.CreatedAt
            })
            .ToList()
    })
    .ToListAsync();

        return Ok(comments);
    }
    [HttpPut("{commentId:guid}")]
    public async Task<IActionResult> UpdateComment(Guid commentId, [FromBody] UpdateCommentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))//Ná»™i dung rá»—ng  tráº£ vá» 400
        {
            return BadRequest(new { message = "Ná»™i dung bÃ¬nh luáº­n khÃ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng" });
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Token khÃ´ng há»£p lá»‡ hoáº·c thiáº¿u UserId" });
        }

        var comment = await _context.Comments // tÃ¬m comment mang id = commentId
            .FirstOrDefaultAsync(x => x.Id == commentId && !x.IsDeleted);

        if (comment == null)// khÃ´ng tÃ¬m tháº¥y comment tráº£ vá» null = 404
        {
            return NotFound(new { message = "KhÃ´ng tÃ¬m tháº¥y bÃ¬nh luáº­n" });
        }

        if (comment.UserId != userId) //Kiá»ƒm tra id check chá»§ comments
        {
            return Forbid();
        }

        var oldContent = comment.Content;

        comment.Content = request.Content.Trim();
        comment.UpdatedAt = DateTime.UtcNow;

        AddAuditLog(
            userId: userId,
            action: "UPDATE",
            entityName: "Comment",
            entityId: comment.Id.ToString(),
            oldValuesJson: $"{{\"content\":\"{oldContent}\"}}",
            newValuesJson: $"{{\"content\":\"{comment.Content}\"}}"
        );

        comment.Content = request.Content.Trim();
        comment.UpdatedAt = DateTime.UtcNow; // Cáº­p nháº­t thá»i gian sá»­a
        AddActivityLog(
            userId: userId,
            taskId: comment.TaskId,
            projectId: comment.ProjectId,
            action: "COMMENT_UPDATED",
            description: "User updated a comment on task",
            metadataJson: $"{{\"commentId\":\"{comment.Id}\"}}"
        );
        // LÆ°u database
        await _context.SaveChangesAsync();

        return Ok(new CommentResponse
        {
            Id = comment.Id,
            TaskId = comment.TaskId,
            ProjectId = comment.ProjectId,
            UserId = comment.UserId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        });
    }


    [HttpDelete("{commentId:guid}")]
    public async Task<IActionResult> DeleteComment(Guid commentId)
    {
        // Láº¥y user id tá»« payload jwt  (json web token)
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Token khÃ´ng há»£p lá»‡ hoáº·c thiáº¿u UserId" });
        }
        //  TÃ¬m comment theo id bá» qua cac comment bá»‹ xÃ³a
        var comment = await _context.Comments
            .FirstOrDefaultAsync(x => x.Id == commentId && !x.IsDeleted);

        if (comment == null)
        {
            return NotFound(new { message = "KhÃ´ng tÃ¬m tháº¥y bÃ¬nh luáº­n" });
        }
        //  Cháº·n xÃ³a comment cá»§a ngÆ°á»i khÃ¡c
        if (comment.UserId != userId)
        {
            return Forbid();
        }

        comment.IsDeleted = true;
        comment.UpdatedAt = DateTime.UtcNow;
        AddActivityLog(
            userId: userId,
            taskId: comment.TaskId,
            projectId: comment.ProjectId,
            action: "COMMENT_DELETED",
            description: "User deleted a comment on task",
            metadataJson: $"{{\"commentId\":\"{comment.Id}\"}}"
        );

        AddAuditLog(
            userId: userId,
            action: "SOFT_DELETE",
            entityName: "Comment",
            entityId: comment.Id.ToString(),
            oldValuesJson: "{\"isDeleted\":false}",
            newValuesJson: "{\"isDeleted\":true}"
        );

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "XÃ³a bÃ¬nh luáº­n thÃ nh cÃ´ng",
            commentId = comment.Id
        });

    }

    //Nhat ki hoat dong tai/sua xoa comment
    private void AddActivityLog(
    Guid userId,
    Guid taskId,
    Guid? projectId,
    string action,
    string description,
    string? metadataJson = null)
    {
        var activityLog = new ActivityLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TaskId = taskId,
            ProjectId = projectId,
            Action = action,
            Description = description,
            MetadataJson = metadataJson,
            CreatedAt = DateTime.UtcNow
        };

        _context.ActivityLogs.Add(activityLog);
    }

    // Notifications
    // 1. Táº¡o 1 báº£n ghi trong Notifications
    // 2. Táº¡o nhiá»u báº£n ghi trong UserNotifications cho tá»«ng ngÆ°á»i Ä‘Æ°á»£c mention
    // 3. Táº¡o NotificationLog Ä‘á»ƒ ghi láº¡i viá»‡c táº¡o thÃ´ng bÃ¡o
    private void CreateMentionNotifications(Comment comment, Guid actorUserId, List<Guid> mentionedUserIds)
    {
        if (mentionedUserIds.Count == 0)
        {
            return;
        }

var notification = new Notification
{
    Id = Guid.NewGuid(),
    TaskId = comment.TaskId,
    ProjectId = comment.ProjectId,
    Title = "Báº¡n Ä‘Æ°á»£c nháº¯c Ä‘áº¿n trong bÃ¬nh luáº­n",
    Message = "CÃ³ ngÆ°á»i Ä‘Ã£ nháº¯c Ä‘áº¿n báº¡n trong má»™t bÃ¬nh luáº­n.",
    Type = "COMMENT_MENTION",
    SourceService = "NotifyService",
    SourceEventId = comment.Id.ToString(),
    CreatedAt = DateTime.UtcNow
};


        foreach (var mentionedUserId in mentionedUserIds)
        {
            notification.UserNotifications.Add(new UserNotification
            {
                Id = Guid.NewGuid(),
                NotificationId = notification.Id,
                UserId = mentionedUserId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        notification.NotificationLogs.Add(new NotificationLog
        {
            Id = Guid.NewGuid(),
            NotificationId = notification.Id,
            UserId = actorUserId,
            Action = "NOTIFICATION_CREATED",
            Status = "Success",
            ErrorMessage = null,
            CreatedAt = DateTime.UtcNow
        });

        _context.Notifications.Add(notification);
    }

    // Ghi log tao sau khi tao  comment
    private void AddAuditLog(

    Guid? userId,
    string action,
    string entityName,
    string? entityId,
    string? oldValuesJson = null,
    string? newValuesJson = null)

    {
        _context.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValuesJson = oldValuesJson,
            NewValuesJson = newValuesJson,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            CreatedAt = DateTime.UtcNow
        });


    }

    [Authorize]
[HttpGet("project/{projectId:guid}")]
public async Task<IActionResult> GetProjectComments(Guid projectId)
{
    var comments = await _context.Comments
        .Where(c => c.ProjectId == projectId && !c.IsDeleted)
        .OrderByDescending(c => c.CreatedAt)
        .Select(c => new
        {
            c.Id,
            c.ProjectId,
            c.UserId,
            c.Content,
            c.CreatedAt,
            c.UpdatedAt
        })
        .ToListAsync();

    return Ok(new { success = true, data = comments });
}

[Authorize]
[HttpPost("project/{projectId:guid}")]
public async Task<IActionResult> CreateProjectComment(Guid projectId, [FromBody] CreateProjectDiscussionRequest request)
{
    var userId = GetCurrentUserId();

    if (string.IsNullOrWhiteSpace(request.Content))
        return BadRequest(new { success = false, message = "Ná»™i dung bÃ¬nh luáº­n khÃ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng" });

    var comment = new Comment
    {
        Id = Guid.NewGuid(),
        ProjectId = projectId,
        UserId = userId,
        Content = request.Content.Trim(),
        CreatedAt = DateTime.UtcNow,
        IsDeleted = false
    };

    _context.Comments.Add(comment);

    foreach (var recipientId in request.RecipientUserIds.Distinct())
    {
        if (recipientId == userId) continue;

        var notification = new Notification
{
    Id = Guid.NewGuid(),
    ProjectId = projectId,
    Title = "CÃ³ tháº£o luáº­n má»›i trong dá»± Ã¡n",
    Message = request.Content.Trim(),
    Type = "project_discussion",
    CreatedAt = DateTime.UtcNow
};

        notification.UserNotifications.Add(new UserNotification
        {
            Id = Guid.NewGuid(),
            NotificationId = notification.Id,
            UserId = recipientId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        _context.Notifications.Add(notification);
    }

    await _context.SaveChangesAsync();

    return Ok(new { success = true, data = comment });
}

private Guid GetCurrentUserId()
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        throw new UnauthorizedAccessException("Token khÃ´ng há»£p lá»‡ hoáº·c thiáº¿u UserId");

    return userId;
}




}

