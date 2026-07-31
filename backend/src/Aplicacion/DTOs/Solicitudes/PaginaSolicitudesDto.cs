namespace Aplicacion.DTOs.Solicitudes;


/// DTO que representa una página de resultados de solicitudes,
/// incluyendo la información necesaria para la paginación.
public sealed record PaginaSolicitudesDto(
    IReadOnlyList<SolicitudListaDto> Items,
    int Page,
    int PageSize,
    int Total,
    int TotalPaginas);
