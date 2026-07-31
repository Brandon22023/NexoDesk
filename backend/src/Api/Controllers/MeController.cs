using Aplicacion.Abstracciones;
using Aplicacion.DTOs.Autenticacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

// Controlador que permite consultar la información del usuario autenticado.
[ApiController]
[Authorize]
[Route("api/v1/me")]
public sealed class MeController(
    IAutenticacionService autenticacionService,
    IUsuarioActual usuarioActual)
    : ControllerBase
{
    /// <summary>Obtiene el perfil del usuario autenticado.</summary>
    /// <remarks>Devuelve los datos de sesión asociados al token JWT enviado.</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(UsuarioSesionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        // Obtiene la información del usuario desde los claims del JWT.
        var contexto = usuarioActual.Obtener();
        
        // Si los claims requeridos no existen, el token no es válido.
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
         // Busca el usuario dentro de la organización indicada por el token.
        var usuario = await autenticacionService.ObtenerUsuarioAsync(
            contexto.UsuarioId,
            contexto.TenantId,
            cancellationToken);
        
        // El usuario puede haber sido desactivado o eliminado después
        // de que el token fue emitido.
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
