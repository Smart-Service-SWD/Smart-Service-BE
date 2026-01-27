namespace SmartService.Application.Abstractions.Auth;

/// <summary>
/// Authentication result containing tokens and user information.
/// </summary>
public record AuthResult(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt,
    Guid UserId,
    string Email,
    string FullName,
    Domain.Entities.UserRole Role);
