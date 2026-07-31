using Aplicacion.Abstracciones;
using Aplicacion.DTOs.Categorias;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

// Controlador encargado de las operaciones relacionadas con categorías.
[ApiController]
[Authorize]
[Route("api/v1/categorias")]
public sealed class CategoriasController(
    ICategoriaConsultaService categoriaConsultaService) : ControllerBase
{
    /// <summary>Lista las categorías activas de la organización actual.</summary>
    /// <remarks>La respuesta se limita al tenant indicado en el token JWT.</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoriaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        // Obtiene las categorías activas desde la capa de aplicación.
        var categorias = await categoriaConsultaService.ListarActivasAsync(
            cancellationToken);

        // Respuesta exitosa con el listado de categorías.
        return Ok(categorias);
    }
}
