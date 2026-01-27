using MediatR;

namespace SmartService.Application.Features.Auth.Commands.Logout;

/// <summary>
/// Command to logout (revoke refresh token).
/// </summary>
public record LogoutCommand(
    string RefreshToken) : IRequest<bool>;
