using System.ComponentModel.DataAnnotations;

namespace TaskService.DTOs.Comments;

public class CommentDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid AuthorId { get; set; }
    public AuthorInfo? Author { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AuthorInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}

public class CreateCommentRequest
{
    [Required, MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
    public List<Guid> MentionedUserIds { get; set; } = new();
}

public class UpdateCommentRequest
{
    [Required, MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
}
