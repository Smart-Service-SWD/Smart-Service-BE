using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartService.Application.Features.Payments.Commands.ConfirmPayment;
using SmartService.Application.Features.Payments.Commands.CreatePayOSPaymentLink;
using SmartService.Application.Features.Payments.Commands.SyncPayOSPaymentStatus;
using SmartService.Domain.ValueObjects;
using System.Security.Claims;

namespace SmartService.API.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IMediator mediator, ILogger<PaymentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

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
        _logger.LogInformation(
            "[Payments API] Create deposit link requested for ServiceRequest {ServiceRequestId}. TraceId={TraceId}",
            serviceRequestId,
            HttpContext.TraceIdentifier);

        var result = await _mediator.Send(new CreatePayOSPaymentLinkCommand(
            serviceRequestId,
            true,
            request.ReturnUrl,
            request.CancelUrl
        ), cancellationToken);

        _logger.LogInformation(
            "[Payments API] Create deposit link succeeded for ServiceRequest {ServiceRequestId}. OrderCode={OrderCode}, LinkStatus={LinkStatus}",
            serviceRequestId,
            result.OrderCode,
            result.Status);

        return Ok(result);
    }

    [HttpPost("{serviceRequestId}/create-final-link")]
    public async Task<IActionResult> CreateFinalLink(
        [FromRoute] Guid serviceRequestId,
        [FromBody] CreateLinkRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "[Payments API] Create final link requested for ServiceRequest {ServiceRequestId}. TraceId={TraceId}",
            serviceRequestId,
            HttpContext.TraceIdentifier);

        var result = await _mediator.Send(new CreatePayOSPaymentLinkCommand(
            serviceRequestId,
            false,
            request.ReturnUrl,
            request.CancelUrl
        ), cancellationToken);

        _logger.LogInformation(
            "[Payments API] Create final link succeeded for ServiceRequest {ServiceRequestId}. OrderCode={OrderCode}, LinkStatus={LinkStatus}",
            serviceRequestId,
            result.OrderCode,
            result.Status);

        return Ok(result);
    }

    [Authorize]
    [HttpPost("{serviceRequestId}/sync-status")]
    public async Task<IActionResult> SyncStatus(
        [FromRoute] Guid serviceRequestId,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var actorUserId))
        {
            throw new UnauthorizedAccessException("Invalid user context.");
        }

        var role = User.FindFirstValue(ClaimTypes.Role);
        var canManageAnyRequest = role == UserRoleConstants.Staff || role == UserRoleConstants.Admin;

        _logger.LogInformation(
            "[Payments API] Sync status requested for ServiceRequest {ServiceRequestId} by User {ActorUserId} (Role={Role}). TraceId={TraceId}",
            serviceRequestId,
            actorUserId,
            role,
            HttpContext.TraceIdentifier);

        var result = await _mediator.Send(
            new SyncPayOSPaymentStatusCommand(serviceRequestId, actorUserId, canManageAnyRequest),
            cancellationToken);

        _logger.LogInformation(
            "[Payments API] Sync status result for ServiceRequest {ServiceRequestId}. Updated={Updated}, PaymentStatus={PaymentStatus}, ServiceRequestStatus={ServiceRequestStatus}, OrderCode={OrderCode}",
            serviceRequestId,
            result.Updated,
            result.PaymentStatus,
            result.ServiceRequestStatus,
            result.OrderCode);

        return Ok(result);
    }
}

public record CreateLinkRequest(string ReturnUrl, string CancelUrl);
