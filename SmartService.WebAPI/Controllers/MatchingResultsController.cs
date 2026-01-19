using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using SmartService.Application.Features.MatchingResults.Commands.Create;

namespace SmartService.API.Controllers;

/// <summary>
/// Controller quản lý kết quả khớp (Matching Results)
/// </summary>
[ApiController]
[Route("api/matching-results")]
[Tags("6. Matching Results - Quản lý kết quả khớp")]
public class MatchingResultsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MatchingResultsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// [CREATE] Tạo mới kết quả khớp
    /// </summary>
    /// <param name="command">Thông tin kết quả khớp cần tạo</param>
    /// <param name="cancellationToken">Token hủy</param>
    /// <returns>ID của kết quả khớp vừa tạo</returns>
    /// <response code="201">Tạo kết quả khớp thành công</response>
    /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Tạo mới kết quả khớp",
        Description = "Tạo một kết quả khớp mới giữa yêu cầu dịch vụ và đại lý dịch vụ với điểm số và mức độ phức tạp hỗ trợ",
        OperationId = "CreateMatchingResult",
        Tags = new[] { "1. CREATE - Tạo mới" })]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMatchingResultCommand command, CancellationToken cancellationToken)
    {
        var matchingResultId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Create), new { id = matchingResultId }, matchingResultId);
    }
}
