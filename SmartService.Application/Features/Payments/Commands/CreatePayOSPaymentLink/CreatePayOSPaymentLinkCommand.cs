using MediatR;
using SmartService.Application.Common.Models.Payments;

namespace SmartService.Application.Features.Payments.Commands.CreatePayOSPaymentLink;

public record CreatePayOSPaymentLinkCommand(
    Guid ServiceRequestId,
    bool IsDeposit,
    string ReturnUrl,
    string CancelUrl
) : IRequest<PaymentLinkResult>;
