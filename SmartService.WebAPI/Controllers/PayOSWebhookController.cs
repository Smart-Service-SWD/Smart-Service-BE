using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PayOS.Models.Webhooks;
using SmartService.Application.Abstractions.Payments;
using SmartService.Application.Features.Payments.Commands.HandlePayOSWebhook;

namespace SmartService.API.Controllers;

[ApiController]
[Route("api/payments/payos-webhook")]
public class PayOSWebhookController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPayOSService _payOSService;
    private readonly ILogger<PayOSWebhookController> _logger;

    public PayOSWebhookController(
        IMediator mediator,
        IPayOSService payOSService,
        ILogger<PayOSWebhookController> logger)
    {
        _mediator = mediator;
        _payOSService = payOSService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Handle([FromBody] Webhook webhookData)
    {
        try 
        {
            _logger.LogInformation("[PayOS Webhook] Incoming webhook request. TraceId={TraceId}", HttpContext.TraceIdentifier);

            var verifiedData = await _payOSService.VerifyWebhook(webhookData);

            _logger.LogInformation(
                "[PayOS Webhook] Verified payload. OrderCode={OrderCode}, Status={Status}, Amount={Amount}, TraceId={TraceId}",
                verifiedData.OrderCode,
                verifiedData.Status,
                verifiedData.Amount,
                HttpContext.TraceIdentifier);

            await _mediator.Send(new HandlePayOSWebhookCommand(verifiedData));

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PayOS Webhook] Failed to process webhook. TraceId={TraceId}", HttpContext.TraceIdentifier);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}
