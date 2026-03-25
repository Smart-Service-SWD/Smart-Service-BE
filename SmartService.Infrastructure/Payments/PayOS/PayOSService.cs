using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartService.Application.Abstractions.Payments;
using SmartService.Domain.Entities;
using ApplicationWebhookData = SmartService.Application.Common.Models.Payments.WebhookData;
using ApplicationPaymentLinkResult = SmartService.Application.Common.Models.Payments.PaymentLinkResult;
using ApplicationPaymentStatusResult = SmartService.Application.Common.Models.Payments.PaymentStatusResult;

namespace SmartService.Infrastructure.Payments.PayOS;

public class PayOSService : IPayOSService
{
    private readonly PayOSClient _payOS;
    private readonly ILogger<PayOSService> _logger;

    public PayOSService(IConfiguration configuration, ILogger<PayOSService> logger)
    {
        var clientId = configuration["PayOS:ClientId"] ?? throw new ArgumentNullException("PayOS:ClientId");
        var apiKey = configuration["PayOS:ApiKey"] ?? throw new ArgumentNullException("PayOS:ApiKey");
        var checksumKey = configuration["PayOS:ChecksumKey"] ?? throw new ArgumentNullException("PayOS:ChecksumKey");

        _payOS = new PayOSClient(clientId, apiKey, checksumKey);
        _logger = logger;
    }

    public async Task<ApplicationPaymentLinkResult> CreatePaymentLink(ServiceRequest request, bool isDeposit, string returnUrl, string cancelUrl)
    {
        var amount = isDeposit 
            ? (int)(request.DepositAmount?.Amount ?? 0) 
            : (int)((request.FinalPrice?.Amount ?? request.EstimatedCost?.Amount ?? 0) - (request.DepositAmount?.Amount ?? 0));

        var description = isDeposit ? "Dat coc dich vu" : "Thanh toan con lai";
        
        // PayOS orderCode must be a number (<= 9007199254740991).
        // Using yyMMddHHmmssfff (15 digits) fits within the limit.
        var orderCode = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff"));

        var item = new PaymentLinkItem {
            Name = request.Description ?? "Dich vu",
            Quantity = 1,
            Price = amount
        };
        var items = new List<PaymentLinkItem> { item };

        var paymentData = new CreatePaymentLinkRequest {
            OrderCode = orderCode,
            Amount = amount,
            Description = description,
            Items = items,
            CancelUrl = cancelUrl,
            ReturnUrl = returnUrl
        };

        _logger.LogInformation(
            "[PayOS] Sending create link request. ServiceRequestId={ServiceRequestId}, IsDeposit={IsDeposit}, Amount={Amount}, OrderCode={OrderCode}",
            request.Id,
            isDeposit,
            amount,
            orderCode);

        try
        {
            var result = await _payOS.PaymentRequests.CreateAsync(paymentData);

            _logger.LogInformation(
                "[PayOS] Create link success. ServiceRequestId={ServiceRequestId}, IsDeposit={IsDeposit}, OrderCode={OrderCode}, Status={Status}",
                request.Id,
                isDeposit,
                result.OrderCode,
                result.Status);

            return new ApplicationPaymentLinkResult(result.OrderCode, result.CheckoutUrl, result.Status.ToString(), result.QrCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[PayOS] Create link failed. ServiceRequestId={ServiceRequestId}, IsDeposit={IsDeposit}, Amount={Amount}, OrderCode={OrderCode}",
                request.Id,
                isDeposit,
                amount,
                orderCode);
            throw;
        }
    }

    public async Task<ApplicationPaymentStatusResult> GetPaymentStatus(long orderCode)
    {
        _logger.LogInformation("[PayOS] Querying payment status for OrderCode {OrderCode}", orderCode);

        var paymentLink = await _payOS.PaymentRequests.GetAsync(orderCode);

        _logger.LogInformation(
            "[PayOS] Payment status response for OrderCode {OrderCode}: Status={Status}, AmountPaid={AmountPaid}",
            orderCode,
            paymentLink.Status,
            paymentLink.AmountPaid);

        return new ApplicationPaymentStatusResult(
            paymentLink.OrderCode,
            paymentLink.Status.ToString().ToUpperInvariant(),
            paymentLink.AmountPaid
        );
    }

    public async Task<ApplicationWebhookData> VerifyWebhook(object webhookData)
    {
        if (webhookData is not Webhook typedData)
            throw new ArgumentException("Invalid webhook data type");

        var verifiedData = await _payOS.Webhooks.VerifyAsync(typedData);
        
        // Map "00" (success) to "PAID" for consistency with internal logic
        var status = verifiedData.Code == "00" ? "PAID" : "FAILED";

        _logger.LogInformation(
            "[PayOS] Verified webhook. OrderCode={OrderCode}, RawCode={RawCode}, MappedStatus={MappedStatus}, Amount={Amount}",
            verifiedData.OrderCode,
            verifiedData.Code,
            status,
            verifiedData.Amount);

        return new ApplicationWebhookData(
            verifiedData.OrderCode,
            (int)verifiedData.Amount,
            verifiedData.Description,
            status,
            typedData.Signature
        );
    }
}
