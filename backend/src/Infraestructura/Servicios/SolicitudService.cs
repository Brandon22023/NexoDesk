using Aplicacion.Abstracciones;
using Aplicacion.DTOs.Autenticacion;
using Aplicacion.DTOs.Solicitudes;
using Aplicacion.Excepciones;
using Dominio.Entidades;
using Dominio.Enums;
using Dominio.Reglas;
using Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Servicios;

public sealed class SolicitudService(
    AppDbContext db,
    IUsuarioActual usuarioActual) : ISolicitudService
{
    public async Task<SolicitudDetalleDto> CrearAsync(
        CrearSolicitudRequest request,
        CancellationToken cancellationToken = default)
    {
        var contexto = ObtenerContexto();
        var prioridad = ParsePrioridad(request.Prioridad);
        var categoria = await ObtenerCategoriaAsync(
            request.CategoriaId!.Value,
            contexto.TenantId,
            cancellationToken);
        var fechaCreacion = DateTime.UtcNow;
        var solicitud = new Solicitud(
            Guid.NewGuid(),
            contexto.TenantId,
            await GenerarCodigoAsync(
                contexto.TenantId, fechaCreacion.Year, cancellationToken),
            request.Titulo.Trim(),
            request.Descripcion.Trim(),
            categoria.Id,
            prioridad,
            EstadoSolicitud.Nueva,
            contexto.UsuarioId,
            agenteId: null,
            fechaCreacion,
            CalculadorSla.CalcularFechaLimite(
                fechaCreacion, categoria.SlaHoras, prioridad));

        await db.Solicitudes.AddAsync(solicitud, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await ObtenerDetalleAsync(solicitud.Id, cancellationToken);
    }

    public async Task<SolicitudDetalleDto> ObtenerDetalleAsync(
        Guid solicitudId,
        CancellationToken cancellationToken = default)
    {
        var contexto = ObtenerContexto();
        var solicitud = await db.Solicitudes
            .AsNoTracking()
            .Include(item => item.Categoria)
            .Include(item => item.Solicitante)
            .Include(item => item.Agente)
            .SingleOrDefaultAsync(
                item => item.Id == solicitudId
                    && item.TenantId == contexto.TenantId,
                cancellationToken);

        if (solicitud is null)
        {
            throw new RecursoNoEncontradoException("La solicitud no existe.");
        }

        if (!ReglasPermisoSolicitud.PuedeVer(
                contexto.Rol,
                contexto.UsuarioId,
                solicitud.SolicitanteId))
        {
            throw new OperacionNoPermitidaException(
                "El usuario no puede consultar esta solicitud.");
        }

        return ToDetalle(solicitud, DateTime.UtcNow);
    }

    public async Task<SolicitudDetalleDto> EditarAsync(
        Guid solicitudId,
        EditarSolicitudRequest request,
        CancellationToken cancellationToken = default)
    {
        var contexto = ObtenerContexto();
        var solicitud = await ObtenerSolicitudTenantAsync(
            solicitudId, contexto.TenantId, cancellationToken);

        if (!ReglasPermisoSolicitud.PuedeEditar(
                contexto.Rol, contexto.UsuarioId,
                solicitud.SolicitanteId, solicitud.Estado))
        {
            throw new OperacionNoPermitidaException(
                "El usuario no puede editar esta solicitud.");
        }

        var prioridad = ParsePrioridad(request.Prioridad);
        var categoria = await ObtenerCategoriaAsync(
            request.CategoriaId!.Value, contexto.TenantId, cancellationToken);
        var debeRecalcular = (solicitud.CategoriaId != categoria.Id
                || solicitud.Prioridad != prioridad)
            && solicitud.Estado is not EstadoSolicitud.Resuelta
            and not EstadoSolicitud.Cerrada
            and not EstadoSolicitud.Cancelada;
        var fechaLimite = debeRecalcular
            ? CalculadorSla.CalcularFechaLimite(
                solicitud.FechaCreacion, categoria.SlaHoras, prioridad)
            : (DateTime?)null;

        solicitud.Editar(
            request.Titulo.Trim(), request.Descripcion.Trim(),
            categoria.Id, prioridad, fechaLimite);
        await db.SaveChangesAsync(cancellationToken);
        return await ObtenerDetalleAsync(solicitud.Id, cancellationToken);
    }

    public async Task<SolicitudDetalleDto> TransicionarAsync(
        Guid solicitudId,
        TransicionSolicitudRequest request,
        CancellationToken cancellationToken = default)
    {
        var contexto = ObtenerContexto();
        var solicitud = await ObtenerSolicitudTenantAsync(
            solicitudId, contexto.TenantId, cancellationToken);
        var accion = ParseAccion(request.Accion);

        if (!ReglasPermisoSolicitud.PuedeEjecutar(
                contexto.Rol, contexto.UsuarioId,
                solicitud.SolicitanteId, accion))
        {
            throw new OperacionNoPermitidaException(
                "El rol del usuario no permite ejecutar esta acción.");
        }

        if (!ReglasTransicionSolicitud.EsPermitida(solicitud.Estado, accion))
        {
            throw new TransicionInvalidaException(
                $"No se puede aplicar '{request.Accion}' sobre una solicitud en estado '{solicitud.Estado}'.");
        }

        await AplicarTransicionAsync(
            solicitud, accion, request, contexto.TenantId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await ObtenerDetalleAsync(solicitud.Id, cancellationToken);
    }
    private ContextoUsuarioActual ObtenerContexto()
    {
        return usuarioActual.Obtener()
            ?? throw new InvalidOperationException(
                "No fue posible obtener el usuario autenticado.");
    }

    private async Task<Categoria> ObtenerCategoriaAsync(
        Guid categoriaId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var categoria = await db.Categorias.AsNoTracking().SingleOrDefaultAsync(
            item => item.Id == categoriaId
                && item.TenantId == tenantId
                && item.Activo,
            cancellationToken);

        return categoria ?? throw new ValidacionException(
            "La categoría indicada no es válida.",
            new Dictionary<string, string[]>
            {
                ["categoriaId"] =
                ["La categoría debe estar activa y pertenecer a la organización."]
            });
    }

    private async Task<Solicitud> ObtenerSolicitudTenantAsync(
        Guid solicitudId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var solicitud = await db.Solicitudes.SingleOrDefaultAsync(
            item => item.Id == solicitudId && item.TenantId == tenantId,
            cancellationToken);

        return solicitud ?? throw new RecursoNoEncontradoException(
            "La solicitud no existe.");
    }

    private static PrioridadSolicitud ParsePrioridad(string value)
    {
        if (Enum.TryParse<PrioridadSolicitud>(
                value,
                ignoreCase: true,
                out var prioridad)
            && Enum.IsDefined(prioridad))
        {
            return prioridad;
        }

        throw new ValidacionException(
            "La prioridad indicada no es válida.",
            new Dictionary<string, string[]>
            {
                ["prioridad"] = ["Valores permitidos: Baja, Media, Alta o Critica."]
            });
    }

    private static AccionSolicitud ParseAccion(string value)
    {
        if (Enum.TryParse<AccionSolicitud>(
                value,
                ignoreCase: true,
                out var accion)
            && Enum.IsDefined(accion))
        {
            return accion;
        }

        throw new ValidacionException(
            "La acción indicada no es válida.",
            new Dictionary<string, string[]>
            {
                ["accion"] =
                ["Valores permitidos: asignar, iniciar, resolver, cerrar, reabrir o cancelar."]
            });
    }

    private static string ValidarMotivo(string? motivo, int minimo)
    {
        if (string.IsNullOrWhiteSpace(motivo)
            || motivo.Trim().Length < minimo)
        {
            throw new MotivoRequeridoException(
                $"El motivo debe tener al menos {minimo} caracteres.");
        }

        return motivo.Trim();
    }

    private async Task AplicarTransicionAsync(
        Solicitud solicitud,
        AccionSolicitud accion,
        TransicionSolicitudRequest request,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        switch (accion)
        {
            case AccionSolicitud.Asignar:
                solicitud.Asignar(await ValidarAgenteAsync(
                    request.AgenteId, tenantId, cancellationToken));
                break;
            case AccionSolicitud.Iniciar:
                solicitud.Iniciar();
                break;
            case AccionSolicitud.Resolver:
                solicitud.Resolver(
                    ValidarMotivo(request.Motivo, 20), DateTime.UtcNow);
                break;
            case AccionSolicitud.Cerrar:
                solicitud.Cerrar();
                break;
            case AccionSolicitud.Reabrir:
                solicitud.Reabrir();
                break;
            case AccionSolicitud.Cancelar:
                solicitud.Cancelar(ValidarMotivo(request.Motivo, 10));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(accion));
        }
    }

    private async Task<Guid> ValidarAgenteAsync(
        Guid? agenteId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        if (!agenteId.HasValue)
        {
            throw new AgenteInvalidoException("Debe indicar un agente válido.");
        }

        var esValido = await db.Usuarios.AsNoTracking().AnyAsync(
            item => item.Id == agenteId.Value
                && item.TenantId == tenantId
                && item.Activo
                && (item.Rol == RolUsuario.Agente
                    || item.Rol == RolUsuario.Admin),
            cancellationToken);

        if (!esValido)
        {
            throw new AgenteInvalidoException(
                "El agente no existe, está inactivo, pertenece a otra organización o no tiene un rol válido.");
        }

        return agenteId.Value;
    }

    private async Task<string> GenerarCodigoAsync(
        Guid tenantId,
        int year,
        CancellationToken cancellationToken)
    {
        var prefijo = $"SOL-{year}-";
        var ultimoCodigo = await db.Solicitudes.AsNoTracking()
            .Where(item => item.TenantId == tenantId
                && item.Codigo.StartsWith(prefijo))
            .OrderByDescending(item => item.Codigo)
            .Select(item => item.Codigo)
            .FirstOrDefaultAsync(cancellationToken);
        var correlativo = 1;

        if (ultimoCodigo is not null
            && int.TryParse(ultimoCodigo[prefijo.Length..], out var ultimo))
        {
            correlativo = ultimo + 1;
        }

        return $"{prefijo}{correlativo:00000}";
    }

    private static SolicitudDetalleDto ToDetalle(
        Solicitud solicitud,
        DateTime ahoraUtc)
    {
        return new SolicitudDetalleDto(
            solicitud.Id, solicitud.Codigo, solicitud.Titulo,
            solicitud.Descripcion, solicitud.Estado.ToString(),
            solicitud.Prioridad.ToString(),
            new ReferenciaSolicitudDto(
                solicitud.Categoria.Id, solicitud.Categoria.Nombre),
            new ReferenciaSolicitudDto(
                solicitud.Solicitante.Id, solicitud.Solicitante.Nombre),
            solicitud.Agente is null ? null : new ReferenciaSolicitudDto(
                solicitud.Agente.Id, solicitud.Agente.Nombre),
            AsUtc(solicitud.FechaCreacion),
            AsUtc(solicitud.FechaLimiteSla),
            CalculadorSla.EstaVencida(
                solicitud.FechaLimiteSla, solicitud.Estado, ahoraUtc),
            solicitud.FechaResolucion.HasValue
                ? AsUtc(solicitud.FechaResolucion.Value)
                : null,
            solicitud.MotivoResolucion,
            solicitud.MotivoCancelacion);
    }

    private static DateTime AsUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
