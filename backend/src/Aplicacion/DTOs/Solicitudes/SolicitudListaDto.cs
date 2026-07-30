namespace Aplicacion.DTOs.Solicitudes;

public sealed record SolicitudListaDto(
    Guid Id,
    string Codigo,
    string Titulo,
    string Estado,
    string Prioridad,
    ReferenciaSolicitudDto Categoria,
    ReferenciaSolicitudDto? Agente,
    DateTime FechaCreacion,
    DateTime FechaLimiteSla,
    bool Vencida);
