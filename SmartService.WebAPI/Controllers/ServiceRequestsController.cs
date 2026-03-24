using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SmartService.API.Contracts;
using SmartService.Application.Features.ServiceRequests.Commands.CancelServiceRequest;
using SmartService.Application.Features.ServiceRequests.Commands.AssignProvider;
using SmartService.Application.Features.ServiceRequests.Commands.Create;
using SmartService.Application.Features.ServiceRequests.Commands.EvaluateComplexity;
using SmartService.Application.Features.ServiceRequests.Commands.StartServiceRequest;
using SmartService.Application.Features.ServiceRequests.Commands.RequestDeposit;
using SmartService.Application.Features.ServiceRequests.Commands.RequestCompletion;
using SmartService.Application.Features.ServiceRequests.Commands.ApproveCompletion;
using SmartService.Application.Features.ServiceRequests.Commands.RejectCompletion;
using SmartService.Application.Features.ServiceRequests.Commands.ConfirmPayment;
using SmartService.Application.Features.ServiceRequests.Commands.RequestFinalPayment;
using SmartService.Application.Features.ServiceRequests.Commands.PayoutServiceRequest;
using SmartService.Domain.ValueObjects;
using System.Security.Claims;

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
            Money.Create(request.EstimatedCost.Amount, request.EstimatedCost.Currency));

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
        var estimatedCost = request.EstimatedCost != null 
            ? Money.Create(request.EstimatedCost.Amount, request.EstimatedCost.Currency) 
            : null;

        var command = new EvaluateServiceComplexityCommand(
            serviceRequestId,
            complexity,
            request.ServiceDefinitionId,
            estimatedCost);

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// [UPDATE] Staff yêu cầu khách hàng đặt cọc
    /// </summary>
    [HttpPatch("{serviceRequestId}/request-deposit")]
    [SwaggerOperation(Summary = "Yêu cầu đặt cọc", OperationId = "RequestDeposit")]
    public async Task<IActionResult> RequestDeposit(
        [FromRoute] Guid serviceRequestId,
        [FromBody] RequestDepositInput input,
        CancellationToken cancellationToken)
    {
        var command = new RequestDepositCommand(
            serviceRequestId,
            Money.Create(input.Amount, input.Currency),
            input.CommissionRate);

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// [UPDATE] Bắt đầu xử lý yêu cầu dịch vụ đã được phân công.
    /// </summary>
    [HttpPatch("{serviceRequestId}/start")]
    [SwaggerOperation(
        Summary = "Bắt đầu xử lý yêu cầu dịch vụ",
        Description = "Chuyển trạng thái yêu cầu từ Assigned sang InProgress",
        OperationId = "StartServiceRequest",
        Tags = new[] { "2. UPDATE - Cập nhật (PATCH)" })]
    [ProducesResponseType(typeof(ServiceRequestStatusResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Start(
        [FromRoute] Guid serviceRequestId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new StartServiceRequestCommand(serviceRequestId),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// [UPDATE] Thợ nộp bằng chứng và yêu cầu hoàn tất công việc.
    /// </summary>
    [HttpPatch("{serviceRequestId}/request-completion")]
    [SwaggerOperation(Summary = "Thợ yêu cầu hoàn tất (kèm bằng chứng)", OperationId = "RequestCompletion")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> RequestCompletion(
        [FromRoute] Guid serviceRequestId,
        [FromForm] string? notes,
        IFormFile? image,
        CancellationToken cancellationToken)
    {
        Stream? imageStream = null;
        if (image != null && image.Length > 0)
        {
            imageStream = image.OpenReadStream();
        }

        var command = new RequestCompletionCommand(
            serviceRequestId,
            new List<CompletionEvidenceDto>(), // URL-based list empty for simple flow
            imageStream,
            image?.FileName);

        await _mediator.Send(command, cancellationToken);
        
        if (imageStream != null) await imageStream.DisposeAsync();
        
        return NoContent();
    }

    /// <summary>
    /// [UPDATE] Staff duyệt hoàn tất công việc.
    /// </summary>
    [HttpPatch("{serviceRequestId}/approve-completion")]
    [SwaggerOperation(Summary = "Staff duyệt hoàn tất", OperationId = "ApproveCompletion")]
    public async Task<IActionResult> ApproveCompletion(
        [FromRoute] Guid serviceRequestId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new ApproveCompletionCommand(serviceRequestId), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// [UPDATE] Staff từ chối hoàn tất công việc.
    /// </summary>
    [HttpPatch("{serviceRequestId}/reject-completion")]
    [SwaggerOperation(Summary = "Staff từ chối hoàn tất", OperationId = "RejectCompletion")]
    public async Task<IActionResult> RejectCompletion(
        [FromRoute] Guid serviceRequestId,
        [FromBody] RejectCompletionRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new RejectCompletionCommand(serviceRequestId, request.Reason), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// [UPDATE] Khách hàng hủy yêu cầu khi staff chưa xác nhận độ phức tạp.
    /// </summary>
    [Authorize(Roles = UserRoleConstants.Customer)]
    [HttpPatch("{serviceRequestId}/cancel")]
    [SwaggerOperation(
        Summary = "Khách hàng hủy yêu cầu dịch vụ",
        Description = "Cho phép khách hàng hủy yêu cầu của chính mình khi staff chưa xác nhận độ phức tạp.",
        OperationId = "CancelServiceRequest",
        Tags = new[] { "2. UPDATE - Cập nhật (PATCH)" })]
    [ProducesResponseType(typeof(ServiceRequestStatusResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(
        [FromRoute] Guid serviceRequestId,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var customerId))
        {
            throw new UnauthorizedAccessException();
        }

        var result = await _mediator.Send(
            new CancelServiceRequestCommand(serviceRequestId, customerId),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// [UPDATE] Staff xác nhận khách đã thanh toán (cọc hoặc trả nốt).
    /// </summary>
    [HttpPatch("{serviceRequestId}/paid")]
    [SwaggerOperation(Summary = "Xác nhận đã thanh toán", OperationId = "ConfirmPaymentPaid")]
    public async Task<IActionResult> ConfirmPaid(
        [FromRoute] Guid serviceRequestId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new ConfirmPaymentCommand(serviceRequestId), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// [UPDATE] Staff yêu cầu khách thanh toán phần còn lại sau khi hoàn thành.
    /// </summary>
    [HttpPatch("{serviceRequestId}/awaiting-payment")]
    [SwaggerOperation(Summary = "Yêu cầu thanh toán cuối", OperationId = "RequestFinalPayment")]
    public async Task<IActionResult> RequestFinalPayment(
        [FromRoute] Guid serviceRequestId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new RequestFinalPaymentCommand(serviceRequestId), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// [POST] Staff tất toán tiền cho thợ.
    /// </summary>
    [HttpPost("{serviceRequestId}/payout")]
    [SwaggerOperation(Summary = "Tất toán cho thợ", OperationId = "PayoutServiceRequest")]
    public async Task<IActionResult> Payout(
        [FromRoute] Guid serviceRequestId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new PayoutServiceRequestCommand(serviceRequestId), cancellationToken);
        return NoContent();
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
    MoneyInput EstimatedCost);

/// <summary>Model yêu cầu đánh giá độ phức tạp của yêu cầu dịch vụ.</summary>
public record EvaluateComplexityRequest(
    EvaluateComplexityRequest.ServiceComplexityInput Complexity,
    Guid? ServiceDefinitionId = null,
    MoneyInput? EstimatedCost = null)
{
    /// <summary>Input đơn giản cho ServiceComplexity (1–5).</summary>
    public record ServiceComplexityInput(int Level);
}

public record RequestDepositInput(decimal Amount, string Currency, decimal CommissionRate);

public record RejectCompletionRequest(string? Reason);
