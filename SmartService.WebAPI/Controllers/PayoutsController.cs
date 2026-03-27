using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartService.Application.Features.Payouts.Commands.ProcessPayout;
using SmartService.Domain.ValueObjects;

namespace SmartService.API.Controllers;

[ApiController]
[Route("api/payouts")]
public class PayoutsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PayoutsController(IMediator mediator)
        => _mediator = mediator;

    [Authorize(Roles = $"{UserRoleConstants.Staff},{UserRoleConstants.Admin}")]
    [HttpPost("process")]
    public async Task<IActionResult> Process([FromBody] ProcessPayoutCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = UserRoleConstants.Agent)]
    [HttpGet("agent/{agentId}")]
    public async Task<IActionResult> GetByAgent([FromRoute] Guid agentId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new Application.Features.Payouts.Queries.GetPayoutsByAgent.GetPayoutsByAgentQuery(agentId), cancellationToken);
        return Ok(result);
    }
}
