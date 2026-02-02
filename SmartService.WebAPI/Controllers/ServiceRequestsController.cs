using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SmartService.Application.Features.ServiceRequests.Commands.AssignProvider;
using SmartService.Application.Features.ServiceRequests.Commands.Create;
using SmartService.Application.Features.ServiceRequests.Commands.EvaluateComplexity;
using SmartService.Application.Features.ServiceRequests.Commands.Start;
using SmartService.Application.Features.ServiceRequests.Commands.Complete;
using SmartService.Application.Features.ServiceRequests.Commands.Cancel;
using SmartService.Application.Features.ServiceRequests.Commands.Update;
using SmartService.Application.Features.ServiceRequests.Commands.Approve;
using SmartService.Application.Features.ServiceRequests.Commands.MatchAgents;

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

    /// <summary>
    /// [UPDATE] Bắt đầu thực hiện yêu cầu dịch vụ đã được gán
    /// </summary>
    /// <param name="serviceRequestId">ID của yêu cầu dịch vụ</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>Không có nội dung trả về</returns>
    /// <response code="204">Bắt đầu thực hiện thành công</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
    /// <response code="404">Không tìm thấy yêu cầu dịch vụ</response>
    [HttpPatch("{serviceRequestId}/start")]
    [SwaggerOperation(
        Summary = "Bắt đầu thực hiện yêu cầu dịch vụ",
        Description = "Chuyển trạng thái yêu cầu dịch vụ từ Assigned sang InProgress",
        OperationId = "StartServiceRequest",
        Tags = new[] { "2. UPDATE - Cập nhật (PATCH)" })]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Start(
        [FromRoute] Guid serviceRequestId,
        CancellationToken cancellationToken)
    {
        var command = new StartServiceRequestCommand(serviceRequestId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// [UPDATE] Hoàn thành yêu cầu dịch vụ
    /// </summary>
    /// <param name="serviceRequestId">ID của yêu cầu dịch vụ</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>Không có nội dung trả về</returns>
    /// <response code="204">Hoàn thành yêu cầu thành công</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
    /// <response code="404">Không tìm thấy yêu cầu dịch vụ</response>
    [HttpPatch("{serviceRequestId}/complete")]
    [SwaggerOperation(
        Summary = "Hoàn thành yêu cầu dịch vụ",
        Description = "Chuyển trạng thái yêu cầu dịch vụ từ InProgress sang Completed",
        OperationId = "CompleteServiceRequest",
        Tags = new[] { "2. UPDATE - Cập nhật (PATCH)" })]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(
        [FromRoute] Guid serviceRequestId,
        CancellationToken cancellationToken)
    {
        var command = new CompleteServiceRequestCommand(serviceRequestId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// [UPDATE] Hủy yêu cầu dịch vụ
    /// </summary>
    /// <param name="serviceRequestId">ID của yêu cầu dịch vụ</param>
    /// <param name="request">Thông tin lý do hủy</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>Không có nội dung trả về</returns>
    /// <response code="204">Hủy yêu cầu thành công</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
    /// <response code="404">Không tìm thấy yêu cầu dịch vụ</response>
    [HttpPatch("{serviceRequestId}/cancel")]
    [SwaggerOperation(
        Summary = "Hủy yêu cầu dịch vụ",
        Description = "Hủy yêu cầu dịch vụ với lý do cụ thể. Không thể hủy yêu cầu đã hoàn thành.",
        OperationId = "CancelServiceRequest",
        Tags = new[] { "2. UPDATE - Cập nhật (PATCH)" })]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(
        [FromRoute] Guid serviceRequestId,
        [FromBody] CancelServiceRequestRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CancelServiceRequestCommand(serviceRequestId, request.CancellationReason);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// [UPDATE] Cập nhật mô tả yêu cầu dịch vụ
    /// </summary>
    /// <param name="serviceRequestId">ID của yêu cầu dịch vụ</param>
    /// <param name="request">Thông tin mô tả mới</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>Không có nội dung trả về</returns>
    /// <response code="204">Cập nhật mô tả thành công</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
    /// <response code="404">Không tìm thấy yêu cầu dịch vụ</response>
    [HttpPatch("{serviceRequestId}/update")]
    [SwaggerOperation(
        Summary = "Cập nhật mô tả yêu cầu dịch vụ",
        Description = "Cập nhật mô tả của yêu cầu dịch vụ. Chỉ có thể cập nhật trước khi được gán.",
        OperationId = "UpdateServiceRequest",
        Tags = new[] { "2. UPDATE - Cập nhật (PATCH)" })]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid serviceRequestId,
        [FromBody] UpdateServiceRequestRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateServiceRequestCommand(serviceRequestId, request.Description);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// [UPDATE] Phê duyệt yêu cầu dịch vụ
    /// </summary>
    /// <param name="serviceRequestId">ID của yêu cầu dịch vụ</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>Không có nội dung trả về</returns>
    /// <response code="204">Phê duyệt thành công</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
    /// <response code="404">Không tìm thấy yêu cầu dịch vụ</response>
    [HttpPatch("{serviceRequestId}/approve")]
    [SwaggerOperation(
        Summary = "Phê duyệt yêu cầu dịch vụ",
        Description = "Chuyển trạng thái yêu cầu dịch vụ từ PendingReview sang Approved",
        OperationId = "ApproveServiceRequest",
        Tags = new[] { "2. UPDATE - Cập nhật (PATCH)" })]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(
        [FromRoute] Guid serviceRequestId,
        CancellationToken cancellationToken)
    {
        var command = new ApproveServiceRequestCommand(serviceRequestId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// [MATCHING] Tìm kiếm agents phù hợp cho yêu cầu dịch vụ
    /// </summary>
    /// <param name="serviceRequestId">ID của yêu cầu dịch vụ</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>Danh sách agents phù hợp với điểm số</returns>
    /// <response code="200">Tìm kiếm thành công</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
    /// <response code="404">Không tìm thấy yêu cầu dịch vụ</response>
    [HttpPost("{serviceRequestId}/match-agents")]
    [SwaggerOperation(
        Summary = "Tìm kiếm agents phù hợp",
        Description = "Sử dụng thuật toán matching để tìm các agents phù hợp dựa trên category và complexity",
        OperationId = "MatchAgentsForServiceRequest",
        Tags = new[] { "3. MATCHING - Tìm kiếm agents" })]
    [ProducesResponseType(typeof(List<AgentMatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MatchAgents(
        [FromRoute] Guid serviceRequestId,
        CancellationToken cancellationToken)
    {
        var command = new MatchAgentsForServiceRequestCommand(serviceRequestId);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
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

/// <summary>
/// Model yêu cầu hủy yêu cầu dịch vụ
/// </summary>
public record CancelServiceRequestRequest(string CancellationReason);

/// <summary>
/// Model yêu cầu cập nhật mô tả yêu cầu dịch vụ
/// </summary>
public record UpdateServiceRequestRequest(string Description);
