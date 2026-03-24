using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.ValueObjects;

namespace SmartService.Application.Features.ServiceRequests.Commands.PayoutServiceRequest;

public class PayoutServiceRequestHandler : IRequestHandler<PayoutServiceRequestCommand>
{
    private readonly IAppDbContext _context;

    public PayoutServiceRequestHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(PayoutServiceRequestCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(x => x.Id == request.ServiceRequestId, cancellationToken);

        if (serviceRequest == null)
            throw new Exception("Service Request not found");

        if (serviceRequest.Status != ServiceStatus.FinalPaymentPaid)
            throw new Exception("Service must be fully paid before payout");

        serviceRequest.MarkAsPayoutCompleted();

        // In a real system, we would create a Payout transaction here
        
        await _context.SaveChangesAsync(cancellationToken);
    }
}
