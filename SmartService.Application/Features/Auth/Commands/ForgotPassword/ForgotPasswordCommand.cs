using MediatR;

namespace SmartService.Application.Features.Auth.Commands.ForgotPassword;

/// <summary>
/// Sends a password reset OTP to the given email address.
/// Always completes silently — does not reveal whether the email exists.
/// </summary>
public sealed record ForgotPasswordCommand(string Email) : IRequest;
