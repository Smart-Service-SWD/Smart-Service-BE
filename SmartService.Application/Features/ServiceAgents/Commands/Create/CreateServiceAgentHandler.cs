using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.ServiceAgents.Commands.Create;

/// <summary>
/// Handler for CreateServiceAgentCommand.
/// Creates a new service agent in the system.
/// </summary>
public class CreateServiceAgentHandler : IRequestHandler<CreateServiceAgentCommand, Guid>
{
    private readonly IAppDbContext _context;

    public CreateServiceAgentHandler(IAppDbContext context)
        => _context = context;

    public async Task<Guid> Handle(CreateServiceAgentCommand request, CancellationToken cancellationToken)
    {
        var agent = ServiceAgent.Create(request.FullName);

        _context.ServiceAgents.Add(agent);
        await _context.SaveChangesAsync(cancellationToken);

        return agent.Id;
    }
}

