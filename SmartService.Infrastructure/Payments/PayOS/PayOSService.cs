using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using Microsoft.Extensions.Configuration;
using SmartService.Application.Abstractions.Payments;
using SmartService.Domain.Entities;
using ApplicationWebhookData = SmartService.Application.Common.Models.Payments.WebhookData;
using ApplicationPaymentLinkResult = SmartService.Application.Common.Models.Payments.PaymentLinkResult;

namespace SmartService.Infrastructure.Payments.PayOS;

public class PayOSService : IPayOSService
{
    private readonly PayOSClient _payOS;

    public PayOSService(IConfiguration configuration)
    {
        var clientId = configuration["PayOS:ClientId"] ?? throw new ArgumentNullException("PayOS:ClientId");
        var apiKey = configuration["PayOS:ApiKey"] ?? throw new ArgumentNullException("PayOS:ApiKey");
        var checksumKey = configuration["PayOS:ChecksumKey"] ?? throw new ArgumentNullException("PayOS:ChecksumKey");

        Console.Error.WriteLine($"[PayOS Config] ClientId: {clientId.Substring(0, 8)}..., ApiKey: {apiKey.Substring(0, 8)}...");
        _payOS = new PayOSClient(clientId, apiKey, checksumKey);
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

        try 
        {
            var result = await _payOS.PaymentRequests.CreateAsync(paymentData);
            return new ApplicationPaymentLinkResult(result.OrderCode, result.CheckoutUrl, result.Status.ToString(), result.QrCode);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[PayOS Error] OrderCode: {orderCode}, Message: {ex.Message}");
            if (ex.InnerException != null)
                Console.Error.WriteLine($"[PayOS Inner Error] {ex.InnerException.Message}");
            throw;
        }
    }

    public async Task<ApplicationWebhookData> VerifyWebhook(object webhookData)
    {
        if (webhookData is not Webhook typedData)
            throw new ArgumentException("Invalid webhook data type");

        var verifiedData = await _payOS.Webhooks.VerifyAsync(typedData);
        return new ApplicationWebhookData(
            verifiedData.OrderCode,
            (int)verifiedData.Amount,
            verifiedData.Description,
            verifiedData.Code,
            typedData.Signature
        );
    }
}
