using MediatR;

namespace SmartService.Application.Features.Auth.Commands.ChangePassword;

/// <summary>
/// Command to change password for an authenticated user.
/// </summary>
public record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword) : IRequest<bool>;
