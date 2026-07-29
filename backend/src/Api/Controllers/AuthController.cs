using Aplicacion.Abstracciones;
using Aplicacion.DTOs.Autenticacion;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(IAutenticacionService autenticacionService)
    : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await autenticacionService.IniciarSesionAsync(
            request,
            cancellationToken);

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

        return Ok(response);
    }
}
