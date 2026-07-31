using Aplicacion.Abstracciones;
using Aplicacion.DTOs.Solicitudes;
using Dominio.Entidades;
using Dominio.Enums;
using Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Servicios;
/// Proporciona consultas paginadas de solicitudes para el usuario autenticado.
public sealed class SolicitudConsultaService(
    AppDbContext db,
    IUsuarioActual usuarioActual) : ISolicitudConsultaService
{
     /// Obtiene las solicitudes aplicando filtros, ordenamiento y paginación.
    public async Task<PaginaSolicitudesDto> ListarAsync(
        FiltroSolicitudes filtro,
        CancellationToken cancellationToken = default)
    {
        var contexto = usuarioActual.Obtener()
            ?? throw new InvalidOperationException(
                "No fue posible obtener el usuario autenticado.");

        var ahoraUtc = DateTime.UtcNow;

        // RN-01: el tenant se toma del JWT y se aplica desde el inicio de la
        // consulta; por eso el listado nunca mezcla organizaciones.
        IQueryable<Solicitud> consulta = db.Solicitudes
            .AsNoTracking()
            .Where(solicitud => solicitud.TenantId == contexto.TenantId);

        if (contexto.Rol == RolUsuario.Solicitante)
        {
            // RN-03: un solicitante no lista las solicitudes de otros usuarios.
            consulta = consulta.Where(
                solicitud => solicitud.SolicitanteId == contexto.UsuarioId);
        }

        if (filtro.Estado.HasValue)
        {
            consulta = consulta.Where(
                solicitud => solicitud.Estado == filtro.Estado.Value);
        }

        if (filtro.Prioridad.HasValue)
        {
            consulta = consulta.Where(
                solicitud => solicitud.Prioridad == filtro.Prioridad.Value);
        }

        if (filtro.CategoriaId.HasValue)
        {
            consulta = consulta.Where(
                solicitud => solicitud.CategoriaId == filtro.CategoriaId.Value);
        }

        if (filtro.AgenteId.HasValue)
        {
            consulta = consulta.Where(
                solicitud => solicitud.AgenteId == filtro.AgenteId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Q))
        {
            var patron = $"%{filtro.Q.Trim()}%";
            consulta = consulta.Where(solicitud =>
                EF.Functions.Like(solicitud.Titulo, patron)
                || EF.Functions.Like(solicitud.Descripcion, patron)
                || EF.Functions.Like(solicitud.Codigo, patron));
        }

        if (filtro.Vencidas.HasValue)
        {
            // RN-04: filtra por límite expirado, excluyendo estados finalizados.
            consulta = filtro.Vencidas.Value
                ? consulta.Where(solicitud =>
                    solicitud.FechaLimiteSla < ahoraUtc
                    && solicitud.Estado != EstadoSolicitud.Resuelta
                    && solicitud.Estado != EstadoSolicitud.Cerrada
                    && solicitud.Estado != EstadoSolicitud.Cancelada)
                : consulta.Where(solicitud =>
                    solicitud.FechaLimiteSla >= ahoraUtc
                    || solicitud.Estado == EstadoSolicitud.Resuelta
                    || solicitud.Estado == EstadoSolicitud.Cerrada
                    || solicitud.Estado == EstadoSolicitud.Cancelada);
        }

        var total = await consulta.CountAsync(cancellationToken);
        consulta = AplicarOrden(consulta, filtro.Sort);

        var filas = await consulta
            .Skip((filtro.Page - 1) * filtro.PageSize)
            .Take(filtro.PageSize)
            .Select(solicitud => new SolicitudListaFila
            {
                Id = solicitud.Id,
                Codigo = solicitud.Codigo,
                Titulo = solicitud.Titulo,
                Estado = solicitud.Estado,
                Prioridad = solicitud.Prioridad,
                CategoriaId = solicitud.Categoria.Id,
                CategoriaNombre = solicitud.Categoria.Nombre,
                AgenteId = solicitud.AgenteId,
                AgenteNombre = solicitud.Agente == null
                    ? null
                    : solicitud.Agente.Nombre,
                FechaCreacion = solicitud.FechaCreacion,
                FechaLimiteSla = solicitud.FechaLimiteSla
            })
            .ToListAsync(cancellationToken);

        var items = filas.Select(fila => new SolicitudListaDto(
                fila.Id,
                fila.Codigo,
                fila.Titulo,
                fila.Estado.ToString(),
                fila.Prioridad.ToString(),
                new ReferenciaSolicitudDto(
                    fila.CategoriaId,
                    fila.CategoriaNombre),
                fila.AgenteId.HasValue
                    ? new ReferenciaSolicitudDto(
                        fila.AgenteId.Value,
                        fila.AgenteNombre ?? string.Empty)
                    : null,
                AsUtc(fila.FechaCreacion),
                AsUtc(fila.FechaLimiteSla),
                fila.FechaLimiteSla < ahoraUtc
                    && fila.Estado is not EstadoSolicitud.Resuelta
                    and not EstadoSolicitud.Cerrada
                    and not EstadoSolicitud.Cancelada))
            .ToArray();

        var totalPaginas = total == 0
            ? 0
            : (int)Math.Ceiling(total / (double)filtro.PageSize);

        return new PaginaSolicitudesDto(
            items,
            filtro.Page,
            filtro.PageSize,
            total,
            totalPaginas);
    }
    /// Aplica el ordenamiento solicitado a la consulta.

    private static IQueryable<Solicitud> AplicarOrden(
        IQueryable<Solicitud> consulta,
        string sort)
    {
        return sort switch
        {
            "fechaCreacion" => consulta.OrderBy(
                solicitud => solicitud.FechaCreacion),
            "-fechaCreacion" => consulta.OrderByDescending(
                solicitud => solicitud.FechaCreacion),
            "prioridad" => consulta.OrderBy(solicitud =>
                solicitud.Prioridad == PrioridadSolicitud.Baja ? 0
                : solicitud.Prioridad == PrioridadSolicitud.Media ? 1
                : solicitud.Prioridad == PrioridadSolicitud.Alta ? 2
                : 3),
            "-prioridad" => consulta.OrderByDescending(solicitud =>
                solicitud.Prioridad == PrioridadSolicitud.Baja ? 0
                : solicitud.Prioridad == PrioridadSolicitud.Media ? 1
                : solicitud.Prioridad == PrioridadSolicitud.Alta ? 2
                : 3),
            "codigo" => consulta.OrderBy(solicitud => solicitud.Codigo),
            _ => throw new ArgumentOutOfRangeException(
                nameof(sort),
                sort,
                "Ordenamiento no soportado.")
        };
    }
    /// Garantiza que una fecha sea tratada como UTC.
    private static DateTime AsUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
    /// Representa una fila interna del listado de solicitudes.

    private sealed class SolicitudListaFila
    {
        public Guid Id { get; init; }

        public string Codigo { get; init; } = string.Empty;

        public string Titulo { get; init; } = string.Empty;

        public EstadoSolicitud Estado { get; init; }

        public PrioridadSolicitud Prioridad { get; init; }

        public Guid CategoriaId { get; init; }

        public string CategoriaNombre { get; init; } = string.Empty;

        public Guid? AgenteId { get; init; }

        public string? AgenteNombre { get; init; }

        public DateTime FechaCreacion { get; init; }

        public DateTime FechaLimiteSla { get; init; }
    }
}
