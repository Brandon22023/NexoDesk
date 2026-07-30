using Dominio.Enums;

namespace Aplicacion.DTOs.Solicitudes;

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
