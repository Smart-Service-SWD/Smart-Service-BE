using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SmartService.Application.Features.ServiceRequests.Commands.AssignProvider;
using SmartService.Application.Features.ServiceRequests.Commands.Create;
using SmartService.Application.Features.ServiceRequests.Commands.EvaluateComplexity;

namespace SmartService.API.Controllers;

/// <summary>
/// Controller quản lý yêu cầu dịch vụ (Service Requests)
/// </summary>
[ApiController]
[Route("api/service-requests")]
[Tags("2. Service Requests - Quản lý yêu cầu dịch vụ")]
public class ServiceRequestsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ServiceRequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// [CREATE] Tạo mới yêu cầu dịch vụ
    /// </summary>
    /// <param name="command">Thông tin yêu cầu dịch vụ cần tạo (bao gồm ComplexityLevel tùy chọn từ 1-5)</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>ID của yêu cầu dịch vụ vừa tạo</returns>
    /// <response code="201">Tạo yêu cầu dịch vụ thành công</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Tạo mới yêu cầu dịch vụ",
        Description = "Tạo một yêu cầu dịch vụ mới trong hệ thống với thông tin khách hàng, danh mục, mô tả và mức độ phức tạp (ComplexityLevel tùy chọn từ 1-5). Nếu có ComplexityLevel, status sẽ là PendingReview, ngược lại là Created.",
        OperationId = "CreateServiceRequest",
        Tags = new[] { "1. CREATE - Tạo mới" })]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateServiceRequestCommand command, CancellationToken cancellationToken)
    {
        var serviceRequestId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Create), new { id = serviceRequestId }, serviceRequestId);
    }

    /// <summary>
    /// [UPDATE] Gán nhà cung cấp cho yêu cầu dịch vụ
    /// </summary>
    /// <param name="serviceRequestId">ID của yêu cầu dịch vụ</param>
    /// <param name="request">Thông tin nhà cung cấp và chi phí ước tính</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>Không có nội dung trả về</returns>
    /// <response code="204">Gán nhà cung cấp thành công</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
    /// <response code="404">Không tìm thấy yêu cầu dịch vụ</response>
    [HttpPatch("{serviceRequestId}/assign-provider")]
    [SwaggerOperation(
        Summary = "Gán nhà cung cấp cho yêu cầu dịch vụ",
        Description = "Cập nhật yêu cầu dịch vụ bằng cách gán một nhà cung cấp cụ thể cùng với chi phí ước tính",
        OperationId = "AssignProvider",
        Tags = new[] { "2. UPDATE - Cập nhật (PATCH)" })]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignProvider(
        [FromRoute] Guid serviceRequestId,
        [FromBody] AssignProviderRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AssignProviderCommand(
            serviceRequestId,
            request.ProviderId,
            request.EstimatedCost);

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// [UPDATE] Đánh giá độ phức tạp của yêu cầu dịch vụ
    /// </summary>
    /// <param name="serviceRequestId">ID của yêu cầu dịch vụ</param>
    /// <param name="request">Thông tin độ phức tạp</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>Không có nội dung trả về</returns>
    /// <response code="204">Đánh giá độ phức tạp thành công</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
    /// <response code="404">Không tìm thấy yêu cầu dịch vụ</response>
    [HttpPatch("{serviceRequestId}/evaluate-complexity")]
    [SwaggerOperation(
        Summary = "Đánh giá độ phức tạp của yêu cầu dịch vụ",
        Description = "Cập nhật mức độ phức tạp của yêu cầu dịch vụ (Low, Medium, High)",
        OperationId = "EvaluateComplexity",
        Tags = new[] { "2. UPDATE - Cập nhật (PATCH)" })]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EvaluateComplexity(
        [FromRoute] Guid serviceRequestId,
        [FromBody] EvaluateComplexityRequest request,
        CancellationToken cancellationToken)
    {
        var command = new EvaluateServiceComplexityCommand(
            serviceRequestId,
            request.Complexity);

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}

/// <summary>
/// Model yêu cầu gán nhà cung cấp cho yêu cầu dịch vụ
/// </summary>
public record AssignProviderRequest(
    Guid ProviderId,
    SmartService.Domain.ValueObjects.Money EstimatedCost);

/// <summary>
/// Model yêu cầu đánh giá độ phức tạp của yêu cầu dịch vụ
/// </summary>
public record EvaluateComplexityRequest(
    SmartService.Domain.ValueObjects.ServiceComplexity Complexity);
