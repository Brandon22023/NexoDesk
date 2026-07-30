using Aplicacion.DTOs.Solicitudes;

namespace Aplicacion.Abstracciones;

public interface ISolicitudService
{
    Task<SolicitudDetalleDto> CrearAsync(
        CrearSolicitudRequest request,
        CancellationToken cancellationToken = default);

    Task<SolicitudDetalleDto> ObtenerDetalleAsync(
        Guid solicitudId,
        CancellationToken cancellationToken = default);

    Task<SolicitudDetalleDto> EditarAsync(
        Guid solicitudId,
        EditarSolicitudRequest request,
        CancellationToken cancellationToken = default);

    Task<SolicitudDetalleDto> TransicionarAsync(
        Guid solicitudId,
        TransicionSolicitudRequest request,
        CancellationToken cancellationToken = default);
}
