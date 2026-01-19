using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.Assignments.Commands.Create;

/// <summary>
/// Handler for CreateAssignmentCommand.
/// Creates a new assignment linking a service request to an agent.
/// </summary>
public class CreateAssignmentHandler : IRequestHandler<CreateAssignmentCommand, Guid>
{
    private readonly IAppDbContext _context;

    public CreateAssignmentHandler(IAppDbContext context)
        => _context = context;

    public async Task<Guid> Handle(CreateAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = Assignment.Create(
            request.ServiceRequestId,
            request.AgentId,
            request.EstimatedCost);

        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);

        return assignment.Id;
    }
}

