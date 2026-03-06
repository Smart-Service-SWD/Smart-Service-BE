using MediatR;

namespace SmartService.Application.Features.Auth.Commands.ResetPassword;

/// <summary>
/// Resets the user's password using the OTP received via email.
/// </summary>
public sealed record ResetPasswordCommand(string Email, string Otp, string NewPassword) : IRequest;
