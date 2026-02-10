using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Exceptions;

namespace SmartService.Application.Features.ServiceRequests.Commands.AssignProvider;

/// <summary>
/// Handler for AssignProviderCommand.
/// Assigns a provider to a service request and updates its status.
/// </summary>
public class AssignProviderHandler : IRequestHandler<AssignProviderCommand, Unit>
{
    private readonly IAppDbContext _context;

    public AssignProviderHandler(IAppDbContext context)
        => _context = context;

    public async Task<Unit> Handle(AssignProviderCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests.FindAsync(
            new object[] { request.ServiceRequestId },
            cancellationToken: cancellationToken);

        if (serviceRequest == null)
            throw new ServiceRequestException.InvalidDescriptionException();

        serviceRequest.AssignProvider(request.ProviderId, request.EstimatedCost);
        
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

