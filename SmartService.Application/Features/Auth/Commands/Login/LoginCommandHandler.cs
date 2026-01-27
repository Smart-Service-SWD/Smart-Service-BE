using MediatR;
using SmartService.Application.Abstractions.Auth;

namespace SmartService.Application.Features.Auth.Commands.Login;

/// <summary>
/// Handler for LoginCommand.
/// Authenticates a user and returns authentication tokens.
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var loginRequest = new LoginRequest(
            Email: request.Email,
            Password: request.Password
        );

        return await _authService.LoginAsync(loginRequest, cancellationToken);
    }
}
