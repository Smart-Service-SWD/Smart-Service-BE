using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.ValueObjects;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.ServiceRequests.Commands.RequestFinalPayment;

public class RequestFinalPaymentHandler : IRequestHandler<RequestFinalPaymentCommand>
{
    private readonly IAppDbContext _context;

    public RequestFinalPaymentHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RequestFinalPaymentCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(x => x.Id == request.ServiceRequestId, cancellationToken);

        if (serviceRequest == null)
            throw new Exception("Service Request not found");

        if (serviceRequest.Status != ServiceStatus.CompletionApproved)
            throw new Exception("Invalid status for requesting final payment");

        serviceRequest.MarkAsAwaitingFinalPayment();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
