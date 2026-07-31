using Api.Contratos.Health;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

// Endpoint de salud utilizado para verificar que la API está funcionando.
[ApiController]
public sealed class HealthController : ControllerBase
{   
    /// <summary>Consulta el estado del servicio.</summary>
    /// <remarks>Endpoint público utilizado por los health checks.</remarks>
    [HttpGet("/health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public ActionResult<HealthResponse> Get()
    {
        // Respuesta simple utilizada por healthchecks y monitoreo.
        return Ok(new HealthResponse("ok"));
    }

    // Ruta heredada conservada para clientes existentes; no forma parte del contrato OpenAPI.
    [HttpGet("/api/v1/health")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public ActionResult<HealthResponse> GetVersionado() => Get();
}
