using Microsoft.AspNetCore.Mvc;
using SmartService.Application.Abstractions.AI;
using SmartService.Application.UseCases.AnalyzeServiceRequest;
using SmartService.Domain.Exceptions;
using Swashbuckle.AspNetCore.Annotations;

namespace SmartService.API.Controllers;

/// <summary>
/// Controller phân tích yêu cầu dịch vụ bằng AI (Ollama).
/// Hỗ trợ phân tích từ text thuần hoặc kết hợp với ảnh (OCR).
/// </summary>
[ApiController]
[Route("api/service-analysis")]
[Tags("3. Service Analysis - Phân tích AI")]
public class ServiceAnalysisController : ControllerBase
{
    private readonly AnalyzeServiceRequestHandler _handler;
    private readonly IOcrService _ocrService;

    public ServiceAnalysisController(
        AnalyzeServiceRequestHandler handler,
        IOcrService ocrService)
    {
        _handler = handler;
        _ocrService = ocrService;
    }

    /// <summary>
    /// [ANALYZE] Phân tích yêu cầu dịch vụ bằng AI (chỉ text)
    /// </summary>
    /// <param name="request">Mô tả yêu cầu dịch vụ</param>
    /// <returns>Kết quả phân tích AI (độ phức tạp, rủi ro, tóm tắt)</returns>
    /// <response code="200">Phân tích thành công</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Phân tích yêu cầu bằng AI (text)",
        Description = "Phân tích mô tả yêu cầu dịch vụ bằng Ollama AI. Trả về độ phức tạp, thông điệp người dùng và chính sách điều phối.",
        OperationId = "AnalyzeServiceRequest")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Analyze([FromBody] AnalysisRequest request)
    {
        if (string.IsNullOrEmpty(request.Description))
            throw new BusinessRuleException.RequiredFieldException(nameof(request.Description));

        var result = await _handler.HandleAsync(request.Description);
        return Ok(result);
    }

    /// <summary>
    /// [ANALYZE] Phân tích yêu cầu dịch vụ bằng AI kết hợp OCR ảnh
    /// </summary>
    /// <remarks>
    /// Nhận multipart/form-data gồm:
    /// - description (string, bắt buộc): mô tả vấn đề của user
    /// - image (file, tùy chọn): ảnh tài liệu/ảnh chụp vấn đề để OCR trích xuất text
    ///
    /// Nếu có ảnh, backend sẽ OCR ảnh → kết hợp với description → gửi cho AI.
    /// Dùng cho category IT và Law nơi user có thể đính kèm ảnh tài liệu.
    /// </remarks>
    /// <response code="200">Phân tích thành công</response>
    /// <response code="400">Thiếu description</response>
    [HttpPost("analyze-with-image")]
    [SwaggerOperation(
        Summary = "Phân tích yêu cầu bằng AI + OCR ảnh",
        Description = "Nhận description (text) + ảnh tùy chọn (multipart). Nếu có ảnh sẽ OCR rồi gộp vào description trước khi gửi AI phân tích. Dùng cho IT và Law.",
        OperationId = "AnalyzeServiceRequestWithImage")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AnalyzeWithImage(
        [FromForm] string description,
        IFormFile? image,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new BusinessRuleException.RequiredFieldException(nameof(description));

        var combinedDescription = description;

        // If image is provided, run OCR and prepend extracted text
        if (image != null && image.Length > 0)
        {
            await using var stream = image.OpenReadStream();
            var ocrText = await _ocrService.ExtractTextAsync(stream, image.FileName, cancellationToken);

            if (!string.IsNullOrWhiteSpace(ocrText))
            {
                combinedDescription = $"[Nội dung trích xuất từ ảnh (OCR)]:\n{ocrText}\n\n[Mô tả người dùng]:\n{description}";
            }
        }

        var result = await _handler.HandleAsync(combinedDescription);
        return Ok(result);
    }
}

/// <summary>Model yêu cầu phân tích AI (chỉ text).</summary>
public record AnalysisRequest(string Description);
