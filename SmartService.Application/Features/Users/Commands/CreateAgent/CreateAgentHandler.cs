using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartService.Application.Abstractions.Auth;
using SmartService.Application.Abstractions.Notifications;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;
using SmartService.Domain.ValueObjects;

namespace SmartService.Application.Features.Users.Commands.CreateAgent;

/// <summary>
/// Handler for CreateAgentCommand.
/// Creates a new agent user, a linked ServiceAgent with capabilities, AuthData entry, and sends welcome email.
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
        // Validate that all referenced ServiceCategories exist
        var categoryIds = request.Capabilities.Select(c => c.CategoryId).Distinct().ToList();
        var existingCategoryIds = await _context.ServiceCategories
            .Where(c => categoryIds.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var missingCategories = categoryIds.Except(existingCategoryIds).ToList();
        if (missingCategories.Count > 0)
            throw new KeyNotFoundException(
                $"Service categories not found: {string.Join(", ", missingCategories)}");

        // Validate that all referenced ServiceDefinitions exist and belong to the correct category
        var allServiceDefIds = request.Capabilities
            .SelectMany(c => c.ServiceIds)
            .Distinct()
            .ToList();

        var existingDefs = await _context.ServiceDefinitions
            .Where(d => allServiceDefIds.Contains(d.Id))
            .Select(d => new { d.Id, d.CategoryId })
            .ToListAsync(cancellationToken);

        var missingDefs = allServiceDefIds.Except(existingDefs.Select(d => d.Id)).ToList();
        if (missingDefs.Count > 0)
            throw new KeyNotFoundException(
                $"Service definitions not found: {string.Join(", ", missingDefs)}");

        foreach (var cap in request.Capabilities)
        {
            foreach (var svcId in cap.ServiceIds)
            {
                var def = existingDefs.First(d => d.Id == svcId);
                if (def.CategoryId != cap.CategoryId)
                    throw new ArgumentException(
                        $"Service definition '{svcId}' does not belong to category '{cap.CategoryId}'.");
            }
        }

        // Create the User with Agent role
        var agentUser = User.CreateAgent(request.FullName, request.Email, request.PhoneNumber);
        _context.Users.Add(agentUser);

        // Create the ServiceAgent linked to this User, with capabilities
        var serviceAgent = ServiceAgent.CreateForUser(request.FullName, agentUser.Id);
        foreach (var capInput in request.Capabilities)
        {
            var complexity = ServiceComplexity.From(capInput.MaxComplexityLevel);
            var capability = AgentCapability.Create(capInput.CategoryId, complexity, capInput.ServiceIds);
            serviceAgent.AddCapability(capability);
        }
        _context.ServiceAgents.Add(serviceAgent);

        await _context.SaveChangesAsync(cancellationToken);

        // Generate temporary password and create AuthData
        var tempPassword = GenerateTemporaryPassword();
        await _authService.CreateAuthDataAsync(agentUser.Id, agentUser.Email, tempPassword, cancellationToken);

        // Send welcome email with credentials
        try
        {
            await _emailService.SendWelcomeEmailAsync(agentUser.Email, agentUser.FullName, tempPassword, cancellationToken);
        }
        catch
        {
            // Email failure should not prevent agent creation
        }

        return agentUser.Id;
    }

    private static string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789@#!";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}

