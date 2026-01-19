using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SmartService.Application.Features.ServiceFeedbacks.Commands.Create;

namespace SmartService.API.Controllers;

/// <summary>
/// Controller quản lý phản hồi dịch vụ (Service Feedbacks)
/// </summary>
[ApiController]
[Route("api/service-feedbacks")]
[Tags("7. Service Feedbacks - Quản lý phản hồi dịch vụ")]
public class ServiceFeedbacksController : ControllerBase
{
    private readonly IMediator _mediator;

    public ServiceFeedbacksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// [CREATE] Tạo mới phản hồi dịch vụ
    /// </summary>
    /// <param name="command">Thông tin phản hồi dịch vụ cần tạo</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>ID của phản hồi dịch vụ vừa tạo</returns>
    /// <response code="201">Tạo phản hồi dịch vụ thành công</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Tạo mới phản hồi dịch vụ",
        Description = "Tạo một phản hồi dịch vụ mới với đánh giá (rating) và nhận xét từ khách hàng",
        OperationId = "CreateServiceFeedback",
        Tags = new[] { "1. CREATE - Tạo mới" })]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateServiceFeedbackCommand command, CancellationToken cancellationToken)
    {
        var feedbackId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Create), new { id = feedbackId }, feedbackId);
    }
}
