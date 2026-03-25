namespace SmartService.Application.Common.Models.Payments;

public record PaymentLinkResult(
    long OrderCode,
    string CheckoutUrl,
    string Status,
    string QrCode
);

public record PaymentStatusResult(
    long OrderCode,
    string Status,
    decimal AmountPaid
);

public record WebhookData(
    long OrderCode,
    decimal Amount,
    string Description,
    string Status,
    string Signature
);
