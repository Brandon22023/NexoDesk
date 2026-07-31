using Aplicacion.Abstracciones;
using Aplicacion.DTOs.Autenticacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;
// Controlador responsable de los endpoints relacionados con autenticación.
[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(IAutenticacionService autenticacionService)
    : ControllerBase
{
    /// <summary>Inicia sesión y genera un token JWT.</summary>
    /// <remarks>Este endpoint es público. Las credenciales inválidas devuelven el formato de error del contrato.</remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        // La lógica de autenticación se delega al servicio.
        var response = await autenticacionService.IniciarSesionAsync(
            request,
            cancellationToken);

        // Si el servicio no encuentra un usuario válido,
        // se devuelve el error definido por el contrato de la API.
        if (response is null)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "No autenticado",
                detail: "El correo o la contraseña no son válidos.",
                extensions: new Dictionary<string, object?>
                {
                    ["codigo"] = "NO_AUTENTICADO"
                });
        }
         // Credenciales válidas: responder con el token y los datos del usuario.
        return Ok(response);
    }
}
