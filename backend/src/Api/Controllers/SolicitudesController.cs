using Api.Contratos.Solicitudes;
using Aplicacion.Abstracciones;
using Aplicacion.DTOs.Solicitudes;
using Dominio.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

// Controlador responsable de la gestión de solicitudes.
// Permite listar, crear, consultar, editar y ejecutar transiciones
// sobre las solicitudes de la organización del usuario autenticado.

[ApiController]
[Authorize]
[Route("api/v1/solicitudes")]
public sealed class SolicitudesController(
    ISolicitudConsultaService solicitudConsultaService,
    ISolicitudService solicitudService,
    IUsuarioActual usuarioActual) : ControllerBase
{   
    // Valores permitidos para el parámetro de ordenamiento.
    private static readonly HashSet<string> SortsPermitidos =
    [
        "fechaCreacion",
        "-fechaCreacion",
        "prioridad",
        "-prioridad",
        "codigo"
    ];
    /// <summary>Lista solicitudes con filtros, ordenamiento y paginación.</summary>
    /// <remarks>Los resultados se restringen al tenant del token y a las solicitudes propias para el rol Solicitante.</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PaginaSolicitudesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
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
    /// <summary>Crea una solicitud para la organización del usuario.</summary>
    /// <remarks>El servidor asigna el código, el estado inicial y calcula la fecha límite del SLA.</remarks>
    [HttpPost]
    [ProducesResponseType(typeof(SolicitudDetalleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> Crear(
        [FromBody] CrearSolicitudRequest request,
        CancellationToken cancellationToken)
    {
        var solicitud = await solicitudService.CrearAsync(
            request, cancellationToken);

        return CreatedAtAction(
            nameof(ObtenerDetalle), new { id = solicitud.Id }, solicitud);
    }
    
    /// <summary>Obtiene el detalle completo de una solicitud.</summary>
    /// <remarks>Un recurso inexistente o perteneciente a otro tenant devuelve 404.</remarks>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SolicitudDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> ObtenerDetalle(
        Guid id,
        CancellationToken cancellationToken)
    {
        var solicitud = await solicitudService.ObtenerDetalleAsync(
            id, cancellationToken);

        return Ok(solicitud);
    }
    
    /// <summary>Edita una solicitud existente.</summary>
    /// <remarks>Actualizar categoría o prioridad recalcula el SLA cuando la solicitud no está resuelta.</remarks>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SolicitudDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> Editar(
        Guid id,
        [FromBody] EditarSolicitudRequest request,
        CancellationToken cancellationToken)
    {
        var solicitud = await solicitudService.EditarAsync(
            id, request, cancellationToken);

        return Ok(solicitud);
    }
    
    /// <summary>Ejecuta una transición del flujo de una solicitud.</summary>
    /// <remarks>Admite asignar, iniciar, resolver, cerrar, reabrir y cancelar, según el estado y el rol del usuario.</remarks>
    [HttpPost("{id:guid}/transiciones")]
    [ProducesResponseType(typeof(SolicitudDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError, "application/problem+json")]
    public async Task<IActionResult> Transicionar(
        Guid id,
        [FromBody] TransicionSolicitudRequest request,
        CancellationToken cancellationToken)
    {
        var solicitud = await solicitudService.TransicionarAsync(
            id, request, cancellationToken);

        return Ok(solicitud);
    }
    
    // Construye una respuesta estándar para errores
    // relacionados con parámetros de consulta inválidos.
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
    
     // Construye una respuesta estándar para solicitudes
    // sin un contexto de autenticación válido.
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
    // Convierte un valor de texto a un enum validando
    // que el valor exista dentro de los permitidos.
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
