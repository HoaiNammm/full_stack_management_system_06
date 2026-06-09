namespace NotifyService.Api.DTOs;

public class UpdateProfileRequest
{
    public string FullName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Department { get; set; }

    public string? Position { get; set; }
}