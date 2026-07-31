using Dominio.Enums;

namespace Aplicacion.DTOs.Solicitudes;


/// DTO que contiene los filtros utilizados para consultar y paginar
/// el listado de solicitudes.
public sealed record FiltroSolicitudes(
    EstadoSolicitud? Estado,
    PrioridadSolicitud? Prioridad,
    Guid? CategoriaId,
    Guid? AgenteId,
    string? Q,
    bool? Vencidas,
    int Page,
    int PageSize,
    string Sort);
