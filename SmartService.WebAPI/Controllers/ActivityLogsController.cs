using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SmartService.Application.Features.ActivityLogs.Commands.Create;

namespace SmartService.API.Controllers;

/// <summary>
/// Controller quản lý nhật ký hoạt động (Activity Logs)
/// </summary>
[ApiController]
[Route("api/activity-logs")]
[Tags("9. Activity Logs - Quản lý nhật ký hoạt động")]
public class ActivityLogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ActivityLogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// [CREATE] Tạo mới nhật ký hoạt động
    /// </summary>
    /// <param name="command">Thông tin nhật ký hoạt động cần tạo</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>ID của nhật ký hoạt động vừa tạo</returns>
    /// <response code="201">Tạo nhật ký hoạt động thành công</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Tạo mới nhật ký hoạt động",
        Description = "Tạo một bản ghi nhật ký hoạt động mới cho yêu cầu dịch vụ với hành động cụ thể",
        OperationId = "CreateActivityLog",
        Tags = new[] { "1. CREATE - Tạo mới" })]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateActivityLogCommand command, CancellationToken cancellationToken)
    {
        var logId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Create), new { id = logId }, logId);
    }
}
