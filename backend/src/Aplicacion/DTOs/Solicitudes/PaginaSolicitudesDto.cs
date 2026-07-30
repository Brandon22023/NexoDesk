namespace Aplicacion.DTOs.Solicitudes;

public sealed record PaginaSolicitudesDto(
    IReadOnlyList<SolicitudListaDto> Items,
    int Page,
    int PageSize,
    int Total,
    int TotalPaginas);
