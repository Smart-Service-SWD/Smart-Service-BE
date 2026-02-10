using SmartService.Application.Abstractions.Auth;

namespace SmartService.Infrastructure.Auth;

/// <summary>
/// Infrastructure-level entity for storing authentication data.
/// This is NOT a domain entity - it's purely for persistence.
/// Separated from User entity to maintain clean architecture.
/// </summary>
public class AuthData : IAuthData
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? EncryptedRefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
