using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Exceptions;

namespace SmartService.Application.Features.ServiceAgents.Commands.Deactivate;

/// <summary>
/// Handler for DeactivateServiceAgentCommand.
/// Deactivates a service agent in the system.
/// </summary>
public class DeactivateServiceAgentHandler : IRequestHandler<DeactivateServiceAgentCommand, Unit>
{
    private readonly IAppDbContext _context;

    public DeactivateServiceAgentHandler(IAppDbContext context)
        => _context = context;

    public async Task<Unit> Handle(DeactivateServiceAgentCommand request, CancellationToken cancellationToken)
    {
        var agent = await _context.ServiceAgents.FindAsync(
            new object[] { request.AgentId },
            cancellationToken: cancellationToken);

        if (agent == null)
            throw new KeyNotFoundException($"ServiceAgent with ID '{request.AgentId}' not found.");

        agent.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

