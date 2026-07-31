using Aplicacion.Abstracciones;
using Aplicacion.DTOs.Categorias;
using Aplicacion.Excepciones;
using Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Servicios;

/// Proporciona consultas de categorías para el usuario autenticado.
public sealed class CategoriaConsultaService(
    AppDbContext db,
    IUsuarioActual usuarioActual) : ICategoriaConsultaService
{

    /// Obtiene las categorías activas de la organización actual.
    public async Task<IReadOnlyList<CategoriaDto>> ListarActivasAsync(
        CancellationToken cancellationToken = default)
    {
        var contexto = usuarioActual.Obtener()
            ?? throw new NoAutenticadoException(
                "No fue posible obtener el usuario autenticado.");

        // RN-01: solo se exponen categorías activas de la organización del JWT.
        return await db.Categorias
            .AsNoTracking()
            .Where(categoria => categoria.TenantId == contexto.TenantId
                && categoria.Activo)
            .OrderBy(categoria => categoria.Nombre)
            .Select(categoria => new CategoriaDto(
                categoria.Id,
                categoria.Nombre,
                categoria.SlaHoras))
            .ToListAsync(cancellationToken);
    }
}
