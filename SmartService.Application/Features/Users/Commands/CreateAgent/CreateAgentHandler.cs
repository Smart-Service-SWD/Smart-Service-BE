using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.Users.Commands.CreateAgent;

/// <summary>
/// Handler for CreateAgentCommand.
/// Creates a new agent user in the system.
/// </summary>
public class CreateAgentHandler : IRequestHandler<CreateAgentCommand, Guid>
{
    private readonly IAppDbContext _context;

    public CreateAgentHandler(IAppDbContext context)
        => _context = context;

    public async Task<Guid> Handle(CreateAgentCommand request, CancellationToken cancellationToken)
    {
        var agent = User.CreateAgent(request.FullName, request.Email, request.PhoneNumber);

        _context.Users.Add(agent);
        await _context.SaveChangesAsync(cancellationToken);

        return agent.Id;
    }
}

