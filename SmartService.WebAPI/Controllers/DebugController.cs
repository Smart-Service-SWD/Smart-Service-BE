
using Microsoft.AspNetCore.Mvc;

namespace SmartService.API.Controllers;

[ApiController]
[Route("api/debug")]
public class DebugController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        Console.Error.WriteLine("[DebugController] Ping received!");
        return Ok(new { Message = "Pong", Time = DateTime.Now });
    }
}
