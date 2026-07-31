namespace Aplicacion.DTOs.Solicitudes;


/// DTO que representa la información resumida de una solicitud
/// para mostrarla en un listado.
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
