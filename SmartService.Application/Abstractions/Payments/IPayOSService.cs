using SmartService.Application.Common.Models.Payments;
using SmartService.Domain.Entities;

namespace SmartService.Application.Abstractions.Payments;

public interface IPayOSService
{
    Task<PaymentLinkResult> CreatePaymentLink(ServiceRequest request, bool isDeposit, string returnUrl, string cancelUrl);
    Task<WebhookData> VerifyWebhook(object webhookData);
}
