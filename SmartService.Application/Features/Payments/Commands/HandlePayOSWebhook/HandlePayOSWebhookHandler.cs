using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;
using SmartService.Domain.ValueObjects;

namespace SmartService.Application.Features.Payments.Commands.HandlePayOSWebhook;

public class HandlePayOSWebhookHandler : IRequestHandler<HandlePayOSWebhookCommand, Unit>
{
    private readonly IAppDbContext _context;

    public HandlePayOSWebhookHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(HandlePayOSWebhookCommand request, CancellationToken cancellationToken)
    {
        // 1. Find the ServiceRequest by PayOSOrderCode
        var serviceRequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(sr => sr.PayOSOrderCode == request.Data.OrderCode, cancellationToken);

        if (serviceRequest == null)
            return Unit.Value; // Or log error: Request not found for this order code

        // 2. Check if payment is successful
        if (request.Data.Status != "PAID")
            return Unit.Value;

        // 3. Automate based on current status
        if (serviceRequest.Status == ServiceStatus.AwaitingDeposit)
        {
            // Automatic Deposit Confirmation
            serviceRequest.ConfirmDeposit();
            // This moves status to Assigned, notifying the agent
        }
        else if (serviceRequest.Status == ServiceStatus.AwaitingFinalPayment)
        {
            // Automatic Final Payment Confirmation
            var amountPaid = Money.Create(request.Data.Amount, "VND");
            serviceRequest.MarkAsFinalPaymentPaid(amountPaid);

            // Automate Payout to Agent Wallet
            if (serviceRequest.AssignedProviderId.HasValue)
            {
                var agent = await _context.ServiceAgents
                    .FirstOrDefaultAsync(a => a.Id == serviceRequest.AssignedProviderId.Value, cancellationToken);

                if (agent != null && serviceRequest.WorkerAmount != null)
                {
                    agent.AddBalance(serviceRequest.WorkerAmount);
                    
                    // Create a Payout record for history
                    var payout = new Payout(
                        serviceRequest.Id,
                        agent.Id,
                        serviceRequest.FinalPrice ?? serviceRequest.EstimatedCost ?? amountPaid,
                        serviceRequest.CommissionRate * 100, // as percentage
                        $"PayOS_{request.Data.OrderCode}"
                    );
                    _context.Payouts.Add(payout);
                    
                    serviceRequest.MarkAsPayoutCompleted();
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
