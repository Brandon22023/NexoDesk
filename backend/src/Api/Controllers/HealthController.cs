using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

// Endpoint de salud utilizado para verificar que la API está funcionando.
[ApiController]
public sealed class HealthController : ControllerBase
{   
    // Devuelve el estado actual del servicio.
    [HttpGet("/api/v1/health")]
    [HttpGet("/health")]
    public IActionResult Get()
    {
        // Respuesta simple utilizada por healthchecks y monitoreo.
        return Ok(new { estado = "ok" });
    }
}
