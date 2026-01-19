using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SmartService.Application.Features.ServiceAttachments.Commands.Create;

namespace SmartService.API.Controllers;

/// <summary>
/// Controller quản lý tệp đính kèm dịch vụ (Service Attachments)
/// </summary>
[ApiController]
[Route("api/service-attachments")]
[Tags("8. Service Attachments - Quản lý tệp đính kèm dịch vụ")]
public class ServiceAttachmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ServiceAttachmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// [CREATE] Tạo mới tệp đính kèm dịch vụ
    /// </summary>
    /// <param name="command">Thông tin tệp đính kèm dịch vụ cần tạo</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>ID của tệp đính kèm dịch vụ vừa tạo</returns>
    /// <response code="201">Tạo tệp đính kèm dịch vụ thành công</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Tạo mới tệp đính kèm dịch vụ",
        Description = "Tạo một tệp đính kèm mới cho yêu cầu dịch vụ với tên tệp, URL và loại tệp đính kèm",
        OperationId = "CreateServiceAttachment",
        Tags = new[] { "1. CREATE - Tạo mới" })]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateServiceAttachmentCommand command, CancellationToken cancellationToken)
    {
        var attachmentId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Create), new { id = attachmentId }, attachmentId);
    }
}
