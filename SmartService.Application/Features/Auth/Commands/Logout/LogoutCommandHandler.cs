using MediatR;
using SmartService.Application.Abstractions.Auth;

namespace SmartService.Application.Features.Auth.Commands.Logout;

/// <summary>
/// Handler for LogoutCommand.
/// Revokes refresh token to logout user.
/// </summary>
public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly IAuthService _authService;

    public LogoutCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        return await _authService.RevokeTokenAsync(request.RefreshToken, cancellationToken);
    }
}
