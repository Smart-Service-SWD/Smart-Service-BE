using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartService.Application.Features.PriceAdjustments.Commands.ApprovePriceAdjustment;
using SmartService.Application.Features.PriceAdjustments.Commands.RejectPriceAdjustment;
using SmartService.Application.Features.PriceAdjustments.Commands.CreatePriceAdjustmentRequest;
using SmartService.Application.Features.PriceAdjustments.Queries.GetPendingPriceAdjustments;
using SmartService.Domain.ValueObjects;
using System.Security.Claims;

namespace SmartService.API.Controllers;

[ApiController]
[Route("api/price-adjustments")]
public class PriceAdjustmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PriceAdjustmentsController(IMediator mediator)
        => _mediator = mediator;

    [Authorize(Roles = UserRoleConstants.Agent)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePriceAdjustmentRequestCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{UserRoleConstants.Staff},{UserRoleConstants.Admin}")]
    [HttpGet("pending")]
    public async Task<ActionResult<List<PriceAdjustmentDto>>> GetPending(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPendingPriceAdjustmentsQuery(), cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{UserRoleConstants.Staff},{UserRoleConstants.Admin}")]
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _mediator.Send(new ApprovePriceAdjustmentCommand(id, userId), cancellationToken);
        return Ok();
    }

    [Authorize(Roles = $"{UserRoleConstants.Staff},{UserRoleConstants.Admin}")]
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _mediator.Send(new RejectPriceAdjustmentCommand(id, userId), cancellationToken);
        return Ok();
    }
}
