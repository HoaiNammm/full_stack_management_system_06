using System.ComponentModel.DataAnnotations;

namespace NotifyService.DTOs.Users;

public class UpdateUserRequest
{
    [MaxLength(100)]
    public string? Name { get; set; }

    // Real URL or a base64 data: URL from client-side avatar upload — no fixed length.
    public string? AvatarUrl { get; set; }
}

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}
