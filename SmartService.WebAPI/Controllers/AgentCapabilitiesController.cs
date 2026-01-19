using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SmartService.Application.Features.AgentCapabilities.Commands.Create;

namespace SmartService.API.Controllers;

/// <summary>
/// Controller quản lý khả năng đại lý (Agent Capabilities)
/// </summary>
[ApiController]
[Route("api/agent-capabilities")]
[Tags("10. Agent Capabilities - Quản lý khả năng đại lý")]
public class AgentCapabilitiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AgentCapabilitiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// [CREATE] Tạo mới khả năng đại lý
    /// </summary>
    /// <param name="command">Thông tin khả năng đại lý cần tạo</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>ID của khả năng đại lý vừa tạo</returns>
    /// <response code="201">Tạo khả năng đại lý thành công</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Tạo mới khả năng đại lý",
        Description = "Thêm một khả năng mới cho đại lý dịch vụ với danh mục và mức độ phức tạp tối đa có thể xử lý",
        OperationId = "CreateAgentCapability",
        Tags = new[] { "1. CREATE - Tạo mới" })]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAgentCapabilityCommand command, CancellationToken cancellationToken)
    {
        var capabilityId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Create), new { id = capabilityId }, capabilityId);
    }
}
