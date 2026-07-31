using Aplicacion.DTOs.Solicitudes;

namespace Aplicacion.Abstracciones;

// Define las operaciones de consulta de solicitudes.
public interface ISolicitudConsultaService
{

    // Obtiene solicitudes aplicando filtros, paginación y ordenamiento.
    Task<PaginaSolicitudesDto> ListarAsync(
        FiltroSolicitudes filtro,
        CancellationToken cancellationToken = default);
}
