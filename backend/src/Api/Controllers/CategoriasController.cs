using Aplicacion.Abstracciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/categorias")]
public sealed class CategoriasController(
    ICategoriaConsultaService categoriaConsultaService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var categorias = await categoriaConsultaService.ListarActivasAsync(
            cancellationToken);

        return Ok(categorias);
    }
}
