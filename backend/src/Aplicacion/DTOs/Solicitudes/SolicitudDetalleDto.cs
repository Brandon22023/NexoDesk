namespace Aplicacion.DTOs.Solicitudes;

public sealed record SolicitudDetalleDto(
    Guid Id,
    string Codigo,
    string Titulo,
    string Descripcion,
    string Estado,
    string Prioridad,
    ReferenciaSolicitudDto Categoria,
    ReferenciaSolicitudDto Solicitante,
    ReferenciaSolicitudDto? Agente,
    DateTime FechaCreacion,
    DateTime FechaLimiteSla,
    bool Vencida,
    DateTime? FechaResolucion,
    string? MotivoResolucion,
    string? MotivoCancelacion);
