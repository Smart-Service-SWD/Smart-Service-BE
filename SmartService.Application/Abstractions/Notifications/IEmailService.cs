namespace SmartService.Application.Abstractions.Notifications;

/// <summary>
/// Email service abstraction for sending system emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a welcome email with temporary password to a newly created user.
    /// </summary>
    Task SendWelcomeEmailAsync(string toEmail, string fullName, string temporaryPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a password reset OTP email to the user.
    /// </summary>
    Task SendPasswordResetEmailAsync(string toEmail, string fullName, string otp, CancellationToken cancellationToken = default);
}
