using Aplicacion.DTOs.Categorias;

namespace Aplicacion.Abstracciones;

public interface ICategoriaConsultaService
{
    Task<IReadOnlyList<CategoriaDto>> ListarActivasAsync(
        CancellationToken cancellationToken = default);
}
