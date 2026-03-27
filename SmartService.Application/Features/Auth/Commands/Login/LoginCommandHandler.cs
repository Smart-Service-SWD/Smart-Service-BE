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
            var linkedAgent = await _context.ServiceAgents
                .FirstOrDefaultAsync(x => x.UserId == authResult.UserId, cancellationToken);

            if (linkedAgent is null)
            {
                var normalizedFullName = authResult.FullName.Trim().ToLower();
                var orphanAgents = await _context.ServiceAgents
                    .Where(x => x.UserId == null && x.FullName.ToLower() == normalizedFullName)
                    .ToListAsync(cancellationToken);

                if (orphanAgents.Count == 1)
                {
                    linkedAgent = orphanAgents[0];
                    linkedAgent.LinkToUser(authResult.UserId);
                }
                else
                {
                    linkedAgent = ServiceAgent.CreateForUser(authResult.FullName, authResult.UserId);
                    _context.ServiceAgents.Add(linkedAgent);
                }
            }

            if (!linkedAgent.IsActive)
            {
                linkedAgent.Activate();
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        return authResult;
    }
}

