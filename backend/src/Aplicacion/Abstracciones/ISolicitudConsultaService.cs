using Aplicacion.DTOs.Solicitudes;

namespace Aplicacion.Abstracciones;

public interface ISolicitudConsultaService
{
    Task<PaginaSolicitudesDto> ListarAsync(
        FiltroSolicitudes filtro,
        CancellationToken cancellationToken = default);
}
