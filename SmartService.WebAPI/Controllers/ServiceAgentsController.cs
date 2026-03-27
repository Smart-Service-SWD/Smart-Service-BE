using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SmartService.Application.Features.ServiceAgents.Commands.Create;
using SmartService.Application.Features.ServiceAgents.Commands.Deactivate;
using SmartService.Application.Features.ServiceAgents.Commands.SetActiveStatus;
using SmartService.Application.Features.ServiceAgents.Commands.UpdateCapabilities;
using SmartService.Application.Features.ServiceAgents.Queries.SearchServiceAgents;
using SmartService.Domain.ValueObjects;
using System.Security.Claims;

namespace SmartService.API.Controllers;

/// <summary>
/// Controller quản lý đại lý dịch vụ (Service Agents)
/// </summary>
[ApiController]
[Route("api/service-agents")]
[Tags("3. Service Agents - Quản lý đại lý dịch vụ")]
public class ServiceAgentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ServiceAgentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// [CREATE] Tạo mới đại lý dịch vụ
    /// </summary>
    /// <param name="command">Thông tin đại lý dịch vụ cần tạo</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>ID của đại lý dịch vụ vừa tạo</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Tạo mới đại lý dịch vụ", OperationId = "CreateServiceAgent")]
    public async Task<IActionResult> Create([FromBody] CreateServiceAgentCommand command, CancellationToken cancellationToken)
    {
        var agentId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Create), new { id = agentId }, agentId);
    }

    [Authorize(Roles = $"{UserRoleConstants.Agent},{UserRoleConstants.Staff},{UserRoleConstants.Admin}")]
    [HttpPatch("{agentId}/active-status")]
    [SwaggerOperation(Summary = "Cập nhật trạng thái hoạt động của thợ")]
    public async Task<IActionResult> SetActiveStatus(
        [FromRoute] Guid agentId,
        [FromBody] UpdateServiceAgentActiveStatusRequest request,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var actorUserId))
        {
            throw new UnauthorizedAccessException();
        }

        var role = User.FindFirstValue(ClaimTypes.Role);
        var canManageAnyAgent = role == UserRoleConstants.Staff || role == UserRoleConstants.Admin;

        var result = await _mediator.Send(
            new SetServiceAgentActiveStatusCommand(agentId, actorUserId, canManageAnyAgent, request.IsActive),
            cancellationToken);

        return Ok(result);
    }

    [Authorize(Roles = $"{UserRoleConstants.Staff},{UserRoleConstants.Admin}")]
    [HttpPut("{agentId}/capabilities")]
    [SwaggerOperation(Summary = "Cập nhật hồ sơ nghề của thợ")]
    public async Task<IActionResult> UpdateCapabilities(
        [FromRoute] Guid agentId,
        [FromBody] UpdateServiceAgentCapabilitiesRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateServiceAgentCapabilitiesCommand(
            agentId,
            request.Capabilities
                .Select(c => new CapabilityInput(c.CategoryId, c.MaxComplexityLevel, c.ServiceIds))
                .ToList());

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{agentId}")]
    [SwaggerOperation(Summary = "Vô hiệu hóa đại lý dịch vụ")]
    public async Task<IActionResult> Deactivate([FromRoute] Guid agentId, CancellationToken cancellationToken)
    {
        var command = new DeactivateServiceAgentCommand(agentId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// [SEARCH] Tìm kiếm thợ với phân trang và sắp xếp theo score
    /// </summary>
    [HttpGet("search")]
    [SwaggerOperation(Summary = "Tìm kiếm thợ với phân trang và sắp xếp")]
    public async Task<IActionResult> Search(
        [FromQuery] Guid? categoryId,
        [FromQuery] Guid? serviceId,
        [FromQuery] int? minComplexity,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchServiceAgentsQuery(categoryId, serviceId, minComplexity, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}

public record UpdateServiceAgentActiveStatusRequest(bool IsActive);
public record UpdateServiceAgentCapabilitiesRequest(IReadOnlyList<UpdateServiceAgentCapabilityItemRequest> Capabilities);
public record UpdateServiceAgentCapabilityItemRequest(Guid CategoryId, int MaxComplexityLevel, IReadOnlyList<Guid> ServiceIds);
