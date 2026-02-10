using MediatR;
using SmartService.Application.Abstractions.Auth;

namespace SmartService.Application.Features.Auth.Commands.Register;

/// <summary>
/// Handler for RegisterCommand.
/// Registers a new user and returns authentication tokens.
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResult>
{
    private readonly IAuthService _authService;

    public RegisterCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var registerRequest = new RegisterRequest(
            Email: request.Email,
            Password: request.Password,
            FullName: request.FullName,
            PhoneNumber: request.PhoneNumber,
            Role: request.Role
        );

        return await _authService.RegisterAsync(registerRequest, cancellationToken);
    }
}
