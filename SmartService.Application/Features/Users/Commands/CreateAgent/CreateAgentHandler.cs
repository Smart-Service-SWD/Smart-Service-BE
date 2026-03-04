using MediatR;
using SmartService.Application.Abstractions.Auth;
using SmartService.Application.Abstractions.Notifications;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.Users.Commands.CreateAgent;

/// <summary>
/// Handler for CreateAgentCommand.
/// Creates a new agent user, AuthData entry, and sends welcome email.
/// </summary>
public class CreateAgentHandler : IRequestHandler<CreateAgentCommand, Guid>
{
    private readonly IAppDbContext _context;
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;

    public CreateAgentHandler(
        IAppDbContext context,
        IAuthService authService,
        IEmailService emailService)
    {
        _context = context;
        _authService = authService;
        _emailService = emailService;
    }

    public async Task<Guid> Handle(CreateAgentCommand request, CancellationToken cancellationToken)
    {
        var agent = User.CreateAgent(request.FullName, request.Email, request.PhoneNumber);

        _context.Users.Add(agent);
        await _context.SaveChangesAsync(cancellationToken);

        // Generate temporary password and create AuthData
        var tempPassword = GenerateTemporaryPassword();
        await _authService.CreateAuthDataAsync(agent.Id, agent.Email, tempPassword, cancellationToken);

        // Send welcome email with credentials
        try
        {
            await _emailService.SendWelcomeEmailAsync(agent.Email, agent.FullName, tempPassword, cancellationToken);
        }
        catch
        {
            // Email failure should not prevent user creation
        }

        return agent.Id;
    }

    private static string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789@#!";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}

