using Api.Contratos.Solicitudes;
using Aplicacion.Abstracciones;
using Aplicacion.DTOs.Solicitudes;
using Dominio.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/solicitudes")]
public sealed class SolicitudesController(
    ISolicitudConsultaService solicitudConsultaService,
    ISolicitudService solicitudService,
    IUsuarioActual usuarioActual) : ControllerBase
{
    private static readonly HashSet<string> SortsPermitidos =
    [
        "fechaCreacion",
        "-fechaCreacion",
        "prioridad",
        "-prioridad",
        "codigo"
    ];

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] ListarSolicitudesQuery query,
        CancellationToken cancellationToken)
    {
        if (usuarioActual.Obtener() is null)
        {
            return NoAutenticado();
        }

        if (query.Page < 1
            || query.PageSize < 1
            || query.PageSize > 100)
        {
            return ParametroInvalido(
                "page debe ser mayor o igual a 1 y pageSize debe estar entre 1 y 100.");
        }

        if (!TryParseEnum<EstadoSolicitud>(query.Estado, out var estado))
        {
            return ParametroInvalido("El parámetro estado no es válido.");
        }

        if (!TryParseEnum<PrioridadSolicitud>(
                query.Prioridad,
                out var prioridad))
        {
            return ParametroInvalido("El parámetro prioridad no es válido.");
        }

        if (!SortsPermitidos.Contains(query.Sort))
        {
            return ParametroInvalido("El parámetro sort no es válido.");
        }

        var filtro = new FiltroSolicitudes(
            estado,
            prioridad,
            query.CategoriaId,
            query.AgenteId,
            query.Q,
            query.Vencidas,
            query.Page,
            query.PageSize,
            query.Sort);

        var resultado = await solicitudConsultaService.ListarAsync(
            filtro,
            cancellationToken);

        return Ok(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] CrearSolicitudRequest request,
        CancellationToken cancellationToken)
    {
        var solicitud = await solicitudService.CrearAsync(
            request, cancellationToken);

        return CreatedAtAction(
            nameof(ObtenerDetalle), new { id = solicitud.Id }, solicitud);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerDetalle(
        Guid id,
        CancellationToken cancellationToken)
    {
        var solicitud = await solicitudService.ObtenerDetalleAsync(
            id, cancellationToken);

        return Ok(solicitud);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Editar(
        Guid id,
        [FromBody] EditarSolicitudRequest request,
        CancellationToken cancellationToken)
    {
        var solicitud = await solicitudService.EditarAsync(
            id, request, cancellationToken);

        return Ok(solicitud);
    }

    [HttpPost("{id:guid}/transiciones")]
    public async Task<IActionResult> Transicionar(
        Guid id,
        [FromBody] TransicionSolicitudRequest request,
        CancellationToken cancellationToken)
    {
        var solicitud = await solicitudService.TransicionarAsync(
            id, request, cancellationToken);

        return Ok(solicitud);
    }

    private ObjectResult ParametroInvalido(string detail)
    {
        return Problem(
            type: "https://mesasitec.local/errores/parametro-invalido",
            statusCode: StatusCodes.Status400BadRequest,
            title: "Parámetro inválido",
            detail: detail,
            extensions: new Dictionary<string, object?>
            {
                ["codigo"] = "PARAMETRO_INVALIDO"
            });
    }

    private ObjectResult NoAutenticado()
    {
        return Problem(
            type: "https://mesasitec.local/errores/no-autenticado",
            statusCode: StatusCodes.Status401Unauthorized,
            title: "No autenticado",
            detail: "El token no contiene los claims requeridos.",
            extensions: new Dictionary<string, object?>
            {
                ["codigo"] = "NO_AUTENTICADO"
            });
    }

    private static bool TryParseEnum<TEnum>(
        string? value,
        out TEnum? result)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = null;
            return true;
        }

        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed)
            && Enum.IsDefined(parsed))
        {
            result = parsed;
            return true;
        }

        result = null;
        return false;
    }
}
