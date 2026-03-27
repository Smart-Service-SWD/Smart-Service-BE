using MediatR;

namespace SmartService.Application.Features.Payments.Commands.SyncPayOSPaymentStatus;

public record SyncPayOSPaymentStatusCommand(
    Guid ServiceRequestId,
    Guid ActorUserId,
    bool CanManageAnyRequest
) : IRequest<SyncPayOSPaymentStatusResult>;

public record SyncPayOSPaymentStatusResult(
    Guid ServiceRequestId,
    string ServiceRequestStatus,
    string PaymentStatus,
    long? OrderCode,
    bool Updated
);
