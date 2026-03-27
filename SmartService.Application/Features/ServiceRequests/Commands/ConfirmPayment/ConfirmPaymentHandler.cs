using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.ValueObjects;
using SmartService.Domain.Entities;

namespace SmartService.Application.Features.ServiceRequests.Commands.ConfirmPayment;

public class ConfirmPaymentHandler : IRequestHandler<ConfirmPaymentCommand>
{
    private readonly IAppDbContext _context;

    public ConfirmPaymentHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(x => x.Id == request.ServiceRequestId, cancellationToken);

        if (serviceRequest == null)
            throw new Exception("Service Request not found");

        if (serviceRequest.Status == ServiceStatus.AwaitingDeposit)
        {
            serviceRequest.ConfirmDeposit();
        }
        else if (serviceRequest.Status == ServiceStatus.AwaitingFinalPayment)
        {
            serviceRequest.MarkAsFinalPaymentPaid(serviceRequest.FinalPrice ?? serviceRequest.EstimatedCost ?? Money.Create(0, "VND"));
        }
        else
        {
            throw new Exception($"Cannot confirm payment for status: {serviceRequest.Status}");
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
