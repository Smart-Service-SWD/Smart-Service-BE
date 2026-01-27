namespace SmartService.Application.Abstractions.Auth;

/// <summary>
/// Token configuration settings loaded from appsettings.json.
/// </summary>
public class TokenConfiguration
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenLifetimeMinutes { get; set; } = 15;
    public int RefreshTokenLifetimeDays { get; set; } = 7;
    public string EncryptionKey { get; set; } = string.Empty;
    public string EncryptionIV { get; set; } = string.Empty;
}
