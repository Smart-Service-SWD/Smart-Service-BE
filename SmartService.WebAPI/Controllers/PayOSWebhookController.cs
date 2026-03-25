using MediatR;
using Microsoft.AspNetCore.Mvc;
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

    public PayOSWebhookController(IMediator mediator, IPayOSService payOSService)
    {
        _mediator = mediator;
        _payOSService = payOSService;
    }

    [HttpPost]
    public async Task<IActionResult> Handle([FromBody] Webhook webhookData)
    {
        try 
        {
            var verifiedData = await _payOSService.VerifyWebhook(webhookData);
            await _mediator.Send(new HandlePayOSWebhookCommand(verifiedData));
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            // Log error
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}
