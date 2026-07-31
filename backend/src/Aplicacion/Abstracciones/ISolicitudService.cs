using Aplicacion.DTOs.Solicitudes;

namespace Aplicacion.Abstracciones;

// Define las operaciones principales para gestionar solicitudes.
public interface ISolicitudService
{

    // Crea una nueva solicitud.
    Task<SolicitudDetalleDto> CrearAsync(
        CrearSolicitudRequest request,
        CancellationToken cancellationToken = default);
    
    // Obtiene el detalle de una solicitud específica.
    Task<SolicitudDetalleDto> ObtenerDetalleAsync(
        Guid solicitudId,
        CancellationToken cancellationToken = default);

    // Actualiza la información editable de una solicitud.
    Task<SolicitudDetalleDto> EditarAsync(
        Guid solicitudId,
        EditarSolicitudRequest request,
        CancellationToken cancellationToken = default);
        
    // Ejecuta cambios de estado sobre una solicitud.
    Task<SolicitudDetalleDto> TransicionarAsync(
        Guid solicitudId,
        TransicionSolicitudRequest request,
        CancellationToken cancellationToken = default);
}
