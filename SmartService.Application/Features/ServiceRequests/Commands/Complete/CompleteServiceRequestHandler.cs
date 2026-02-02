using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Exceptions;

namespace SmartService.Application.Features.ServiceRequests.Commands.Complete;

/// <summary>
/// Handler for CompleteServiceRequestCommand.
/// Completes a service request that is in progress.
/// </summary>
public class CompleteServiceRequestHandler : IRequestHandler<CompleteServiceRequestCommand, Unit>
{
    private readonly IAppDbContext _context;

    public CompleteServiceRequestHandler(IAppDbContext context)
        => _context = context;

    public async Task<Unit> Handle(CompleteServiceRequestCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests.FindAsync(
            new object[] { request.ServiceRequestId },
            cancellationToken: cancellationToken);

        if (serviceRequest == null)
            throw new DomainException("Service request not found.");

        serviceRequest.Complete();
        
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
