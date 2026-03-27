using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.Entities;
using SmartService.Domain.ValueObjects;

namespace SmartService.Application.Features.Payments.Commands.HandlePayOSWebhook;

public class HandlePayOSWebhookHandler : IRequestHandler<HandlePayOSWebhookCommand, Unit>
{
    private readonly IAppDbContext _context;
    private readonly ILogger<HandlePayOSWebhookHandler> _logger;

    public HandlePayOSWebhookHandler(
        IAppDbContext context,
        ILogger<HandlePayOSWebhookHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Unit> Handle(HandlePayOSWebhookCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[PayOS Webhook] Received webhook for OrderCode {OrderCode}. Status={Status}, Amount={Amount}",
            request.Data.OrderCode,
            request.Data.Status,
            request.Data.Amount);

        // 1. Find the ServiceRequest by PayOSOrderCode
        var serviceRequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(sr => sr.PayOSOrderCode == request.Data.OrderCode, cancellationToken);

        if (serviceRequest == null)
        {
            _logger.LogWarning(
                "[PayOS Webhook] ServiceRequest not found for OrderCode {OrderCode}. Status={Status}, Amount={Amount}",
                request.Data.OrderCode,
                request.Data.Status,
                request.Data.Amount);
            return Unit.Value;
        }

        var previousStatus = serviceRequest.Status;

        // 2. Check if payment is successful (matches "PAID" from our service mapping or "00" from raw PayOS)
        if (request.Data.Status != "PAID" && request.Data.Status != "00")
        {
            _logger.LogWarning(
                "[PayOS Webhook] Ignored non-success payment for ServiceRequest {ServiceRequestId} (OrderCode {OrderCode}). Status={Status}",
                serviceRequest.Id,
                request.Data.OrderCode,
                request.Data.Status);
            return Unit.Value;
        }

        var expectedDepositAmount = serviceRequest.DepositAmount?.Amount ?? 0m;
        var expectedFinalAmount =
            Math.Max(
                0m,
                (serviceRequest.FinalPrice?.Amount ?? serviceRequest.EstimatedCost?.Amount ?? 0m) -
                (serviceRequest.DepositAmount?.Amount ?? 0m));

        // 3. Automate based on current status
        if (serviceRequest.Status == ServiceStatus.AwaitingDeposit)
        {
            if (request.Data.Amount + 0.01m < expectedDepositAmount)
            {
                _logger.LogWarning(
                    "[PayOS Webhook] Ignored underpaid deposit for ServiceRequest {ServiceRequestId}. OrderCode={OrderCode}, Paid={Paid}, ExpectedDeposit={ExpectedDeposit}",
                    serviceRequest.Id,
                    request.Data.OrderCode,
                    request.Data.Amount,
                    expectedDepositAmount);
                return Unit.Value;
            }

            // Automatic Deposit Confirmation
            serviceRequest.ConfirmDeposit();
            // This moves status to Assigned, notifying the agent
        }
        else if (serviceRequest.Status == ServiceStatus.AwaitingFinalPayment)
        {
            if (request.Data.Amount + 0.01m < expectedFinalAmount)
            {
                _logger.LogWarning(
                    "[PayOS Webhook] Ignored payment because amount does not cover final due for ServiceRequest {ServiceRequestId}. OrderCode={OrderCode}, Paid={Paid}, ExpectedFinal={ExpectedFinal}",
                    serviceRequest.Id,
                    request.Data.OrderCode,
                    request.Data.Amount,
                    expectedFinalAmount);
                return Unit.Value;
            }

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
        else
        {
            _logger.LogInformation(
                "[PayOS Webhook] No state transition for ServiceRequest {ServiceRequestId} (OrderCode {OrderCode}) because current status is {CurrentStatus}",
                serviceRequest.Id,
                request.Data.OrderCode,
                serviceRequest.Status);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "[PayOS Webhook] Processed ServiceRequest {ServiceRequestId} for OrderCode {OrderCode}. Status: {PreviousStatus} -> {CurrentStatus}",
            serviceRequest.Id,
            request.Data.OrderCode,
            previousStatus,
            serviceRequest.Status);

        return Unit.Value;
    }
}
