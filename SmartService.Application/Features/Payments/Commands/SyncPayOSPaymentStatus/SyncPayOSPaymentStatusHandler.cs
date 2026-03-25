using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartService.Application.Abstractions.Payments;
using SmartService.Application.Abstractions.Persistence;
using SmartService.Domain.ValueObjects;

namespace SmartService.Application.Features.Payments.Commands.SyncPayOSPaymentStatus;

public class SyncPayOSPaymentStatusHandler : IRequestHandler<SyncPayOSPaymentStatusCommand, SyncPayOSPaymentStatusResult>
{
    private readonly IAppDbContext _context;
    private readonly IPayOSService _payOSService;
    private readonly ILogger<SyncPayOSPaymentStatusHandler> _logger;

    public SyncPayOSPaymentStatusHandler(
        IAppDbContext context,
        IPayOSService payOSService,
        ILogger<SyncPayOSPaymentStatusHandler> logger)
    {
        _context = context;
        _payOSService = payOSService;
        _logger = logger;
    }

    public async Task<SyncPayOSPaymentStatusResult> Handle(SyncPayOSPaymentStatusCommand request, CancellationToken cancellationToken)
    {
        var serviceRequest = await _context.ServiceRequests
            .FirstOrDefaultAsync(sr => sr.Id == request.ServiceRequestId, cancellationToken);

        if (serviceRequest == null)
            throw new KeyNotFoundException($"ServiceRequest with ID '{request.ServiceRequestId}' not found.");

        if (!request.CanManageAnyRequest && serviceRequest.CustomerId != request.ActorUserId)
            throw new UnauthorizedAccessException("You do not have permission to sync this payment.");

        var paymentStatus = "NO_ORDER_CODE";
        var updated = false;
        var previousStatus = serviceRequest.Status;

        if (serviceRequest.PayOSOrderCode.HasValue)
        {
            _logger.LogInformation(
                "[PayOS Sync] Checking payment status for ServiceRequest {ServiceRequestId} with OrderCode {OrderCode}. CurrentStatus={CurrentStatus}",
                serviceRequest.Id,
                serviceRequest.PayOSOrderCode.Value,
                serviceRequest.Status);

            var status = await _payOSService.GetPaymentStatus(serviceRequest.PayOSOrderCode.Value);
            paymentStatus = status.Status;

            if (string.Equals(status.Status, "PAID", StringComparison.OrdinalIgnoreCase))
            {
                if (serviceRequest.Status == ServiceStatus.AwaitingDeposit)
                {
                    var expectedDepositAmount = serviceRequest.DepositAmount?.Amount ?? 0m;
                    if (status.AmountPaid + 0.01m < expectedDepositAmount)
                    {
                        _logger.LogWarning(
                            "[PayOS Sync] Ignored underpaid deposit for ServiceRequest {ServiceRequestId}. OrderCode={OrderCode}, Paid={Paid}, ExpectedDeposit={ExpectedDeposit}",
                            serviceRequest.Id,
                            serviceRequest.PayOSOrderCode.Value,
                            status.AmountPaid,
                            expectedDepositAmount);
                        return new SyncPayOSPaymentStatusResult(
                            serviceRequest.Id,
                            serviceRequest.Status.ToString(),
                            paymentStatus,
                            serviceRequest.PayOSOrderCode,
                            updated
                        );
                    }

                    serviceRequest.ConfirmDeposit();
                    updated = true;
                }
                else if (serviceRequest.Status == ServiceStatus.AwaitingFinalPayment)
                {
                    var expectedFinalAmount = Math.Max(
                        0m,
                        (serviceRequest.FinalPrice?.Amount ?? serviceRequest.EstimatedCost?.Amount ?? 0m) -
                        (serviceRequest.DepositAmount?.Amount ?? 0m));
                    if (status.AmountPaid + 0.01m < expectedFinalAmount)
                    {
                        _logger.LogWarning(
                            "[PayOS Sync] Ignored paid status because amount does not cover final due for ServiceRequest {ServiceRequestId}. OrderCode={OrderCode}, Paid={Paid}, ExpectedFinal={ExpectedFinal}",
                            serviceRequest.Id,
                            serviceRequest.PayOSOrderCode.Value,
                            status.AmountPaid,
                            expectedFinalAmount);
                        return new SyncPayOSPaymentStatusResult(
                            serviceRequest.Id,
                            serviceRequest.Status.ToString(),
                            paymentStatus,
                            serviceRequest.PayOSOrderCode,
                            updated
                        );
                    }

                    var finalPaid = status.AmountPaid > 0
                        ? Money.Create(status.AmountPaid, "VND")
                        : serviceRequest.FinalPrice ?? serviceRequest.EstimatedCost ?? Money.Create(0, "VND");

                    serviceRequest.MarkAsFinalPaymentPaid(finalPaid);
                    updated = true;
                }
            }
        }

        if (updated)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            "[PayOS Sync] Result for ServiceRequest {ServiceRequestId}. OrderCode={OrderCode}, PaymentStatus={PaymentStatus}, Updated={Updated}, Status={PreviousStatus}->{CurrentStatus}",
            serviceRequest.Id,
            serviceRequest.PayOSOrderCode,
            paymentStatus,
            updated,
            previousStatus,
            serviceRequest.Status);

        return new SyncPayOSPaymentStatusResult(
            serviceRequest.Id,
            serviceRequest.Status.ToString(),
            paymentStatus,
            serviceRequest.PayOSOrderCode,
            updated
        );
    }
}
