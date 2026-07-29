using System.Security.Claims;
using Aplicacion.Abstracciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/me")]
public sealed class MeController(IAutenticacionService autenticacionService)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue("sub"), out var usuarioId)
            || !Guid.TryParse(
                User.FindFirstValue("tenantId"),
                out var tenantId))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "No autenticado",
                detail: "El token no contiene los claims requeridos.",
                extensions: new Dictionary<string, object?>
                {
                    ["codigo"] = "NO_AUTENTICADO"
                });
        }

        var usuario = await autenticacionService.ObtenerUsuarioAsync(
            usuarioId,
            tenantId,
            cancellationToken);

        if (usuario is null)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "No autenticado",
                detail: "El usuario ya no está activo.",
                extensions: new Dictionary<string, object?>
                {
                    ["codigo"] = "NO_AUTENTICADO"
                });
        }

        return Ok(usuario);
    }
}
