using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SmartService.Application.Features.Assignments.Commands.Create;

namespace SmartService.API.Controllers;

/// <summary>
/// Controller quản lý phân công (Assignments)
/// </summary>
[ApiController]
[Route("api/assignments")]
[Tags("5. Assignments - Quản lý phân công")]
public class AssignmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AssignmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// [CREATE] Tạo mới phân công
    /// </summary>
    /// <param name="command">Thông tin phân công cần tạo</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>ID của phân công vừa tạo</returns>
    /// <response code="201">Tạo phân công thành công</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Tạo mới phân công",
        Description = "Tạo một phân công mới giữa yêu cầu dịch vụ và đại lý với chi phí ước tính",
        OperationId = "CreateAssignment",
        Tags = new[] { "1. CREATE - Tạo mới" })]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAssignmentCommand command, CancellationToken cancellationToken)
    {
        var assignmentId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Create), new { id = assignmentId }, assignmentId);
    }
}
