using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
public sealed class HealthController : ControllerBase
{
    [HttpGet("/api/v1/health")]
    [HttpGet("/health")]
    public IActionResult Get()
    {
        return Ok(new { estado = "ok" });
    }
}
