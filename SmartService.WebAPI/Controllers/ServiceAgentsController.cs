using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SmartService.Application.Features.ServiceAgents.Commands.Create;
using SmartService.Application.Features.ServiceAgents.Commands.Deactivate;
using SmartService.Application.Features.ServiceAgents.Commands.SetActiveStatus;
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
    /// <response code="201">Tạo đại lý dịch vụ thành công</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Tạo mới đại lý dịch vụ",
        Description = "Tạo một đại lý dịch vụ mới trong hệ thống",
        OperationId = "CreateServiceAgent",
        Tags = new[] { "1. CREATE - Tạo mới" })]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateServiceAgentCommand command, CancellationToken cancellationToken)
    {
        var agentId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Create), new { id = agentId }, agentId);
    }

    [Authorize(Roles = $"{UserRoleConstants.Agent},{UserRoleConstants.Staff},{UserRoleConstants.Admin}")]
    [HttpPatch("{agentId}/active-status")]
    [SwaggerOperation(
        Summary = "Cập nhật trạng thái hoạt động của thợ",
        Description = "Cho phép thợ bật/tắt nhận việc mới. Staff/Admin cũng có thể cập nhật trạng thái cho thợ.",
        OperationId = "SetServiceAgentActiveStatus",
        Tags = new[] { "2. UPDATE - Cập nhật (PATCH)" })]
    [ProducesResponseType(typeof(ServiceAgentActiveStatusResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        var canManageAnyAgent =
            role == UserRoleConstants.Staff || role == UserRoleConstants.Admin;

        var result = await _mediator.Send(
            new SetServiceAgentActiveStatusCommand(
                agentId,
                actorUserId,
                canManageAnyAgent,
                request.IsActive),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// [DELETE] Vô hiệu hóa đại lý dịch vụ
    /// </summary>
    /// <param name="agentId">ID của đại lý dịch vụ cần vô hiệu hóa</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>Không có nội dung trả về</returns>
    /// <response code="204">Vô hiệu hóa đại lý dịch vụ thành công</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
    /// <response code="404">Không tìm thấy đại lý dịch vụ</response>
    [HttpDelete("{agentId}")]
    [SwaggerOperation(
        Summary = "Vô hiệu hóa đại lý dịch vụ",
        Description = "Xóa/vô hiệu hóa một đại lý dịch vụ khỏi hệ thống",
        OperationId = "DeactivateServiceAgent",
        Tags = new[] { "3. DELETE - Xóa" })]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate([FromRoute] Guid agentId, CancellationToken cancellationToken)
    {
        var command = new DeactivateServiceAgentCommand(agentId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}

public record UpdateServiceAgentActiveStatusRequest(bool IsActive);
