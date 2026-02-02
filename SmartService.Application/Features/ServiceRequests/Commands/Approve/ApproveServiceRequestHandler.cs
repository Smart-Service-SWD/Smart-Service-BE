using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Exceptions;

namespace SmartService.Application.Features.ServiceRequests.Commands.Approve;

/// <summary>
/// Handler for ApproveServiceRequestCommand.
/// Approves a service request that is pending review.
/// </summary>
public class ApproveServiceRequestHandler : IRequestHandler<ApproveServiceRequestCommand, Unit>
{
    private readonly IAppDbContext _context;

    public ApproveServiceRequestHandler(IAppDbContext context)
        => _context = context;

    public async Task<Unit> Handle(ApproveServiceRequestCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests.FindAsync(
            new object[] { request.ServiceRequestId },
            cancellationToken: cancellationToken);

        if (serviceRequest == null)
            throw new DomainException("Service request not found.");

        serviceRequest.Approve();
        
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
