using Aplicacion.Abstracciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/me")]
public sealed class MeController(
    IAutenticacionService autenticacionService,
    IUsuarioActual usuarioActual)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var contexto = usuarioActual.Obtener();

        if (contexto is null)
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
            contexto.UsuarioId,
            contexto.TenantId,
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
