using MediatR;
using SmartService.Application.Abstractions.Persistence;

namespace SmartService.Application.Features.ServiceAgents.Commands.SetActiveStatus;

public class SetServiceAgentActiveStatusHandler
    : IRequestHandler<SetServiceAgentActiveStatusCommand, ServiceAgentActiveStatusResult>
{
    private readonly IAppDbContext _context;

    public SetServiceAgentActiveStatusHandler(IAppDbContext context)
        => _context = context;

    public async Task<ServiceAgentActiveStatusResult> Handle(
        SetServiceAgentActiveStatusCommand request,
        CancellationToken cancellationToken)
    {
        var agent = await _context.ServiceAgents.FindAsync(
            new object[] { request.AgentId },
            cancellationToken: cancellationToken);

        if (agent == null)
            throw new KeyNotFoundException($"ServiceAgent with ID '{request.AgentId}' not found.");

        if (!request.CanManageAnyAgent && agent.UserId != request.ActorUserId)
            throw new UnauthorizedAccessException();

        if (request.IsActive)
        {
            agent.Activate();
        }
        else
        {
            agent.Deactivate();
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new ServiceAgentActiveStatusResult(agent.Id, agent.IsActive);
    }
}
