using Microsoft.AspNetCore.Mvc;
using SmartService.Application.Abstractions.AI;
using Swashbuckle.AspNetCore.Annotations;

namespace SmartService.API.Controllers;

/// <summary>
/// Controller cung cấp chức năng OCR (trích xuất văn bản từ ảnh) sử dụng Tesseract.
/// </summary>
[ApiController]
[Route("api/ocr")]
[Tags("9. OCR - Nhận dạng ký tự quang học")]
public class OcrController : ControllerBase
{
    private readonly IOcrService _ocrService;

    public OcrController(IOcrService ocrService)
    {
        _ocrService = ocrService;
    }

    /// <summary>
    /// [OCR] Trích xuất văn bản từ ảnh
    /// </summary>
    /// <param name="file">File ảnh cần quét (JPEG, PNG, BMP,...)</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>Văn bản được trích xuất từ ảnh</returns>
    /// <response code="200">Trích xuất văn bản thành công</response>
    /// <response code="400">File không hợp lệ hoặc không có file</response>
    [HttpPost("scan")]
    [SwaggerOperation(
        Summary = "Trích xuất văn bản từ ảnh (OCR)",
        Description = "Nhận ảnh từ multipart/form-data, dùng Tesseract OCR để trích xuất văn bản. Hỗ trợ tiếng Việt và English. Dùng cho category IT và Law.",
        OperationId = "OcrScan",
        Tags = new[] { "OCR" })]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(OcrScanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Scan(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "Vui lòng upload ảnh hợp lệ." });

        await using var stream = file.OpenReadStream();
        var text = await _ocrService.ExtractTextAsync(stream, file.FileName, cancellationToken);

        var wordCount = string.IsNullOrWhiteSpace(text)
            ? 0
            : text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

        return Ok(new OcrScanResponse(
            Success: !string.IsNullOrWhiteSpace(text),
            Text: text,
            WordCount: wordCount
        ));
    }
}

/// <summary>Kết quả OCR trả về cho client.</summary>
public record OcrScanResponse(
    bool Success,
    string Text,
    int WordCount);
