using MediatR;
using SmartService.Application.Abstractions.Auth;

namespace SmartService.Application.Features.Auth.Commands.ChangePassword;

/// <summary>
/// Handler for ChangePasswordCommand.
/// </summary>
public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly IAuthService _authService;

    public ChangePasswordCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        await _authService.ChangePasswordAsync(
            request.UserId,
            request.CurrentPassword,
            request.NewPassword,
            cancellationToken);

        return true;
    }
}
