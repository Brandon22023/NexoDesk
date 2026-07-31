using Aplicacion.Abstracciones;
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
    // Devuelve las categorías activas pertenecientes a la organización
    // del usuario autenticado.
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        // Obtiene las categorías activas desde la capa de aplicación.
        var categorias = await categoriaConsultaService.ListarActivasAsync(
            cancellationToken);

        // Respuesta exitosa con el listado de categorías.
        return Ok(categorias);
    }
}
