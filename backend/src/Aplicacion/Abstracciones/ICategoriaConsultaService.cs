using Aplicacion.DTOs.Categorias;

namespace Aplicacion.Abstracciones;

// Define las operaciones de consulta relacionadas con categorías.
public interface ICategoriaConsultaService
{

    // Obtiene las categorías activas disponibles para el usuario actual.
    Task<IReadOnlyList<CategoriaDto>> ListarActivasAsync(
        CancellationToken cancellationToken = default);
}
