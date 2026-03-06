namespace SmartService.Application.Abstractions.Auth;

/// <summary>
/// Authentication service abstraction.
/// Handles user authentication, token generation, and refresh token management.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user and returns authentication tokens.
    /// </summary>
    Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user and returns authentication tokens.
    /// </summary>
    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes access token using a valid refresh token.
    /// </summary>
    Task<AuthResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a refresh token (logout).
    /// </summary>
    Task<bool> RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies if a refresh token is valid.
    /// </summary>
    Task<bool> VerifyRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an AuthData entry for an existing user (used when admin creates accounts).
    /// Returns the generated temporary (plain-text) password.
    /// </summary>
    Task<string> CreateAuthDataAsync(Guid userId, string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the password for an authenticated user.
    /// Requires the current password to verify identity.
    /// </summary>
    Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a 6-digit OTP and sends a password reset email to the user.
    /// Always returns successfully (does not reveal whether the email exists).
    /// </summary>
    Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets the user's password using the OTP received via email.
    /// </summary>
    Task ResetPasswordAsync(string email, string otp, string newPassword, CancellationToken cancellationToken = default);
}
