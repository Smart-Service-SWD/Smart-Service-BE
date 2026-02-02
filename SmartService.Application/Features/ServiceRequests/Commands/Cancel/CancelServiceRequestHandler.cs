using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Exceptions;

namespace SmartService.Application.Features.ServiceRequests.Commands.Cancel;

/// <summary>
/// Handler for CancelServiceRequestCommand.
/// Cancels a service request with a reason.
/// </summary>
public class CancelServiceRequestHandler : IRequestHandler<CancelServiceRequestCommand, Unit>
{
    private readonly IAppDbContext _context;

    public CancelServiceRequestHandler(IAppDbContext context)
        => _context = context;

    public async Task<Unit> Handle(CancelServiceRequestCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests.FindAsync(
            new object[] { request.ServiceRequestId },
            cancellationToken: cancellationToken);

        if (serviceRequest == null)
            throw new DomainException("Service request not found.");

        serviceRequest.Cancel(request.CancellationReason);
        
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
