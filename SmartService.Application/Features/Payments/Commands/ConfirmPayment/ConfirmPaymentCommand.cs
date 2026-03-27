namespace SmartService.Application.Features.Payments.Commands.ConfirmPayment;

public record ConfirmPaymentCommand(
    Guid ServiceRequestId,
    string? PaymentReference,
    bool IsDeposit = false) : MediatR.IRequest<MediatR.Unit>;
