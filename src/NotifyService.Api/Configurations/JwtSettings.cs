namespace NotifyService.Api.Configurations;

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int ExpirationHours { get; set; } = 24;

    public int RefreshTokenExpirationDays { get; set; } = 7;

    public int PasswordResetExpirationMinutes { get; set; } = 15;
}