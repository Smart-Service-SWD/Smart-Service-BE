using Microsoft.AspNetCore.Mvc;
using SmartService.Application.UseCases.AnalyzeServiceRequest;

namespace SmartService.API.Controllers;

[ApiController]
[Route("api/service-analysis")]
public class ServiceAnalysisController : ControllerBase
{
    private readonly AnalyzeServiceRequestHandler _handler;

    public ServiceAnalysisController(AnalyzeServiceRequestHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
public async Task<IActionResult> Analyze([FromBody] AnalysisRequest request)
{
    if (string.IsNullOrEmpty(request.Description))
        return BadRequest();

    // Kết quả bây giờ bao gồm cả contextDescription và dispatchPolicy
    var result = await _handler.HandleAsync(request.Description);
    return Ok(result);
}

public record AnalysisRequest(string Description);
}

