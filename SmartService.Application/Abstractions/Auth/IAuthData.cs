namespace SmartService.Application.Abstractions.Auth;

/// <summary>
/// Abstraction for authentication data.
/// This interface allows Application layer to work with auth data
/// without depending on Infrastructure implementation.
/// </summary>
public interface IAuthData
{
    Guid Id { get; }
    Guid UserId { get; }
    string Email { get; }
    string PasswordHash { get; }
    string? EncryptedRefreshToken { get; }
    DateTime? RefreshTokenExpiresAt { get; }
    DateTime CreatedAt { get; }
    DateTime? UpdatedAt { get; }
}
