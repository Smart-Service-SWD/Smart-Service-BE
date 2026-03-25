using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartService.Application.Features.Payments.Commands.ConfirmPayment;
using SmartService.Application.Features.Payments.Commands.CreatePayOSPaymentLink;
using SmartService.Domain.ValueObjects;

namespace SmartService.API.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
        => _mediator = mediator;

    [Authorize(Roles = $"{UserRoleConstants.Staff},{UserRoleConstants.Admin}")]
    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] ConfirmPaymentCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return Ok();
    }

    [HttpPost("{serviceRequestId}/create-deposit-link")]
    public async Task<IActionResult> CreateDepositLink(
        [FromRoute] Guid serviceRequestId,
        [FromBody] CreateLinkRequest request,
        CancellationToken cancellationToken)
    {
        Console.Error.WriteLine($"[PaymentsController] CreateDepositLink called for {serviceRequestId}");
        var result = await _mediator.Send(new CreatePayOSPaymentLinkCommand(
            serviceRequestId,
            true,
            request.ReturnUrl,
            request.CancelUrl
        ), cancellationToken);

        return Ok(result);
    }

    [HttpPost("{serviceRequestId}/create-final-link")]
    public async Task<IActionResult> CreateFinalLink(
        [FromRoute] Guid serviceRequestId,
        [FromBody] CreateLinkRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreatePayOSPaymentLinkCommand(
            serviceRequestId,
            false,
            request.ReturnUrl,
            request.CancelUrl
        ), cancellationToken);

        return Ok(result);
    }
}

public record CreateLinkRequest(string ReturnUrl, string CancelUrl);

