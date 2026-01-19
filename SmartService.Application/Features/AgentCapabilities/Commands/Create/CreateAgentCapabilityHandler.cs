using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.AgentCapabilities.Commands.Create;

/// <summary>
/// Handler for CreateAgentCapabilityCommand.
/// Adds a new capability to the agent's skill set.
/// </summary>
public class CreateAgentCapabilityHandler : IRequestHandler<CreateAgentCapabilityCommand, Guid>
{
    private readonly IAppDbContext _context;

    public CreateAgentCapabilityHandler(IAppDbContext context)
        => _context = context;

    public async Task<Guid> Handle(CreateAgentCapabilityCommand request, CancellationToken cancellationToken)
    {
        var capability = AgentCapability.Create(request.CategoryId, request.MaxComplexity);

        _context.AgentCapabilities.Add(capability);
        await _context.SaveChangesAsync(cancellationToken);

        return capability.Id;
    }
}

