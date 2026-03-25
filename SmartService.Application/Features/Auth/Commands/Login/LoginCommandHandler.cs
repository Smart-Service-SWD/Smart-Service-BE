using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartService.Application.Abstractions.Auth;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.Auth.Commands.Login;

/// <summary>
/// Handler for LoginCommand.
/// Authenticates a user and returns authentication tokens.
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly IAuthService _authService;
    private readonly IAppDbContext _context;

    public LoginCommandHandler(IAuthService authService, IAppDbContext context)
    {
        _authService = authService;
        _context = context;
    }

    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var loginRequest = new LoginRequest(
            Email: request.Email,
            Password: request.Password
        );

        var authResult = await _authService.LoginAsync(loginRequest, cancellationToken);

        if (authResult.Role == UserRole.Agent)
        {
            var linkedAgentExists = await _context.ServiceAgents
                .AnyAsync(x => x.UserId == authResult.UserId, cancellationToken);

            if (!linkedAgentExists)
            {
                _context.ServiceAgents.Add(ServiceAgent.CreateForUser(authResult.FullName, authResult.UserId));
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        return authResult;
    }
}
