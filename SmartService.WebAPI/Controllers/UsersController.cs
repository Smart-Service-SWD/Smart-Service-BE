using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SmartService.Application.Features.Users.Commands.CreateAgent;
using SmartService.Application.Features.Users.Commands.CreateCustomer;
using SmartService.Application.Features.Users.Commands.CreateStaff;

namespace SmartService.API.Controllers;

/// <summary>
/// Controller quản lý người dùng (Users)
/// </summary>
[ApiController]
[Route("api/users")]
[Tags("1. Users - Quản lý người dùng")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// [CREATE] Tạo mới khách hàng (Customer)
    /// </summary>
    /// <param name="command">Thông tin khách hàng cần tạo</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>ID của khách hàng vừa tạo</returns>
    /// <response code="201">Tạo khách hàng thành công</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
    [HttpPost("customers")]
    [SwaggerOperation(
        Summary = "Tạo mới khách hàng",
        Description = "Tạo một khách hàng mới trong hệ thống với đầy đủ thông tin: họ tên, email, số điện thoại",
        OperationId = "CreateCustomer",
        Tags = new[] { "1. CREATE - Tạo mới" })]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerCommand command, CancellationToken cancellationToken)
    {
        var userId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(CreateCustomer), new { id = userId }, userId);
    }

    /// <summary>
    /// [CREATE] Tạo mới đại lý (Agent)
    /// </summary>
    /// <param name="command">Thông tin đại lý cần tạo</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>ID của đại lý vừa tạo</returns>
    /// <response code="201">Tạo đại lý thành công</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
    [HttpPost("agents")]
    [SwaggerOperation(
        Summary = "Tạo mới đại lý",
        Description = "Tạo một đại lý mới trong hệ thống với đầy đủ thông tin: họ tên, email, số điện thoại",
        OperationId = "CreateAgent",
        Tags = new[] { "1. CREATE - Tạo mới" })]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAgent([FromBody] CreateAgentCommand command, CancellationToken cancellationToken)
    {
        var userId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(CreateAgent), new { id = userId }, userId);
    }

    /// <summary>
    /// [CREATE] Tạo mới nhân viên (Staff)
    /// </summary>
    /// <param name="command">Thông tin nhân viên cần tạo</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>ID của nhân viên vừa tạo</returns>
    /// <response code="201">Tạo nhân viên thành công</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
    [HttpPost("staff")]
    [SwaggerOperation(
        Summary = "Tạo mới nhân viên",
        Description = "Tạo một nhân viên mới trong hệ thống với đầy đủ thông tin: họ tên, email, số điện thoại",
        OperationId = "CreateStaff",
        Tags = new[] { "1. CREATE - Tạo mới" })]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateStaff([FromBody] CreateStaffCommand command, CancellationToken cancellationToken)
    {
        var userId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(CreateStaff), new { id = userId }, userId);
    }
}
