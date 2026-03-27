using MediatR;
using SmartService.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SmartService.Application.Features.ServiceRequests.Commands.RequestDeposit;

public class RequestDepositHandler : IRequestHandler<RequestDepositCommand, Unit>
{
    private readonly IAppDbContext _context;

    public RequestDepositHandler(IAppDbContext context)
        => _context = context;

    public async Task<Unit> Handle(RequestDepositCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests.FindAsync(
            new object[] { request.ServiceRequestId },
            cancellationToken: cancellationToken);

        if (serviceRequest == null)
            throw new KeyNotFoundException($"ServiceRequest with ID '{request.ServiceRequestId}' not found.");

        var amount = request.DepositAmount;
        var rate = request.CommissionRate;

        // Auto-fetch settings if not fully provided
        if (rate <= 0 || (amount != null && amount.Amount <= 0))
        {
            var settings = await _context.CommissionSettings
                .FirstOrDefaultAsync(s => s.ServiceDefinitionId == serviceRequest.ServiceDefinitionId, cancellationToken);

            if (settings != null)
            {
                if (rate <= 0) rate = settings.CommissionPercent / 100m;
                if (amount == null || amount.Amount <= 0)
                {
                    var total = serviceRequest.EstimatedCost?.Amount ?? 0;
                    var depositAmt = total * (settings.DepositPercent / 100m);
                    amount = SmartService.Domain.ValueObjects.Money.Create(depositAmt, serviceRequest.EstimatedCost?.Currency ?? "VND");
                }
            }
        }

        serviceRequest.RequestDeposit(amount ?? SmartService.Domain.ValueObjects.Money.Create(0, "VND"), rate);

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
