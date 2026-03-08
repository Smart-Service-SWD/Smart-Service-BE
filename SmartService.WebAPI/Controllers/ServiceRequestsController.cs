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
    /// [CREATE] Tạo mới yêu cầu dịch vụ (kèm OCR + AI phân tích tự động)
    /// </summary>
    /// <remarks>
    /// Nhận **multipart/form-data** với các field:
    /// - `customerId` (Guid, bắt buộc)
    /// - `categoryId` (Guid, bắt buộc)
    /// - `serviceDefinitionId` (Guid, bắt buộc) – dịch vụ cụ thể mà khách chọn
    /// - `description` (string, bắt buộc) – mô tả vấn đề
    /// - `addressText` (string, tuỳ chọn)
    /// - `image` (file, tuỳ chọn) – ảnh tài liệu/lỗi để OCR trích xuất text
    ///
    /// **Backend sẽ tự động:**
    /// 1. OCR ảnh (nếu có) → trích xuất text
    /// 2. Gộp text OCR + description → gửi AI phân tích
    /// 3. Lưu ServiceRequest với complexity từ AI
    /// 4. Lưu ServiceAttachment (nếu có ảnh)
    /// 5. Trả về kết quả gồm cả AI analysis
    /// </remarks>
    /// <response code="201">Tạo yêu cầu thành công, kèm kết quả AI</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Tạo yêu cầu dịch vụ (OCR + AI tự động)",
        Description = "Nhận multipart/form-data gồm description + ảnh tùy chọn. Backend tự OCR → AI → lưu DB trong 1 request.",
        OperationId = "CreateServiceRequest",
        Tags = new[] { "1. CREATE - Tạo mới" })]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(CreateServiceRequestResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromForm] CreateServiceRequestFormInput input,
        IFormFile? image,
        CancellationToken cancellationToken)
    {
        Stream? imageStream = null;
        if (image != null && image.Length > 0)
        {
            imageStream = image.OpenReadStream();
        }

        var command = new CreateServiceRequestCommand(
            CustomerId: input.CustomerId,
            CategoryId: input.CategoryId,
            ServiceDefinitionId: input.ServiceDefinitionId,
            Description: input.Description,
            AddressText: input.AddressText,
            // ComplexityLevel intentionally omitted – AI sets it automatically
            ImageStream: imageStream,
            ImageFileName: image?.FileName);

        var result = await _mediator.Send(command, cancellationToken);

        // Dispose stream after use
        if (imageStream != null) await imageStream.DisposeAsync();

        return CreatedAtAction(nameof(Create), new { id = result.ServiceRequestId }, result);
    }

    /// <summary>
    /// [UPDATE] Gán nhà cung cấp cho yêu cầu dịch vụ
    /// </summary>
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
    [HttpPatch("{serviceRequestId}/evaluate-complexity")]
    [SwaggerOperation(
        Summary = "Đánh giá độ phức tạp của yêu cầu dịch vụ",
        Description = "Cập nhật mức độ phức tạp của yêu cầu dịch vụ (Low, Medium, High)",
        OperationId = "EvaluateComplexity",
        Tags = new[] { "2. UPDATE - Cập nhật (PATCH)" })]
    [ProducesResponseType(typeof(EvaluateServiceComplexityResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EvaluateComplexity(
        [FromRoute] Guid serviceRequestId,
        [FromBody] EvaluateComplexityRequest request,
        CancellationToken cancellationToken)
    {
        var complexity = SmartService.Domain.ValueObjects.ServiceComplexity.From(request.Complexity.Level);

        var command = new EvaluateServiceComplexityCommand(
            serviceRequestId,
            complexity);

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}

// ── Inline record models ──────────────────────────────────────────────────────

/// <summary>Form input cho Create Service Request (multipart/form-data).
/// ComplexityLevel không còn là input – AI tự phân tích và set.</summary>
public record CreateServiceRequestFormInput(
    Guid CustomerId,
    Guid CategoryId,
    Guid ServiceDefinitionId,
    string Description,
    string? AddressText = null);

/// <summary>Model yêu cầu gán nhà cung cấp cho yêu cầu dịch vụ.</summary>
public record AssignProviderRequest(
    Guid ProviderId,
    SmartService.Domain.ValueObjects.Money EstimatedCost);

/// <summary>Model yêu cầu đánh giá độ phức tạp của yêu cầu dịch vụ.</summary>
public record EvaluateComplexityRequest(
    EvaluateComplexityRequest.ServiceComplexityInput Complexity)
{
    /// <summary>Input đơn giản cho ServiceComplexity (1–5).</summary>
    public record ServiceComplexityInput(int Level);
}
