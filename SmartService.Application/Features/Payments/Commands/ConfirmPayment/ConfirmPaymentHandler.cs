using MediatR;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace SmartService.Application.Features.Payments.Commands.ConfirmPayment;

public class ConfirmPaymentHandler : IRequestHandler<ConfirmPaymentCommand, Unit>
{
    private readonly IAppDbContext _context;
    private readonly IMediator _mediator;

    public ConfirmPaymentHandler(IAppDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<MediatR.Unit> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(sr => sr.Id == request.ServiceRequestId, cancellationToken);

        if (serviceRequest == null)
            throw new KeyNotFoundException($"ServiceRequest with ID '{request.ServiceRequestId}' not found.");

        if (request.IsDeposit)
        {
            serviceRequest.ConfirmDeposit();
        }
        else
        {
            serviceRequest.MarkAsFinalPaymentPaid(serviceRequest.FinalPrice ?? serviceRequest.EstimatedCost ?? Money.Create(0, "VND"));
        }

        await _context.SaveChangesAsync(cancellationToken);

        return MediatR.Unit.Value;
    }
}
