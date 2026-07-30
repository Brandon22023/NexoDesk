using Dominio.Enums;

namespace Dominio.Entidades;

public sealed class Solicitud
{
    private Solicitud()
    {
    }

    public Solicitud(
        Guid id,
        Guid tenantId,
        string codigo,
        string titulo,
        string descripcion,
        Guid categoriaId,
        PrioridadSolicitud prioridad,
        EstadoSolicitud estado,
        Guid solicitanteId,
        Guid? agenteId,
        DateTime fechaCreacion,
        DateTime fechaLimiteSla,
        DateTime? fechaResolucion = null,
        string? motivoResolucion = null,
        string? motivoCancelacion = null)
    {
        Id = id;
        TenantId = tenantId;
        Codigo = codigo;
        Titulo = titulo;
        Descripcion = descripcion;
        CategoriaId = categoriaId;
        Prioridad = prioridad;
        Estado = estado;
        SolicitanteId = solicitanteId;
        AgenteId = agenteId;
        FechaCreacion = fechaCreacion;
        FechaLimiteSla = fechaLimiteSla;
        FechaResolucion = fechaResolucion;
        MotivoResolucion = motivoResolucion;
        MotivoCancelacion = motivoCancelacion;
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string Codigo { get; private set; } = string.Empty;

    public string Titulo { get; private set; } = string.Empty;

    public string Descripcion { get; private set; } = string.Empty;

    public Guid CategoriaId { get; private set; }

    public PrioridadSolicitud Prioridad { get; private set; }

    public EstadoSolicitud Estado { get; private set; }

    public Guid SolicitanteId { get; private set; }

    public Guid? AgenteId { get; private set; }

    public DateTime FechaCreacion { get; private set; }

    public DateTime FechaLimiteSla { get; private set; }

    public DateTime? FechaResolucion { get; private set; }

    public string? MotivoResolucion { get; private set; }

    public string? MotivoCancelacion { get; private set; }

    public Tenant Tenant { get; private set; } = null!;

    public Categoria Categoria { get; private set; } = null!;

    public Usuario Solicitante { get; private set; } = null!;

    public Usuario? Agente { get; private set; }

    public void Editar(
        string titulo,
        string descripcion,
        Guid categoriaId,
        PrioridadSolicitud prioridad,
        DateTime? nuevaFechaLimiteSla)
    {
        Titulo = titulo;
        Descripcion = descripcion;
        CategoriaId = categoriaId;
        Prioridad = prioridad;

        if (nuevaFechaLimiteSla.HasValue)
        {
            FechaLimiteSla = nuevaFechaLimiteSla.Value;
        }
    }

    public void Asignar(Guid agenteId)
    {
        AgenteId = agenteId;
        Estado = EstadoSolicitud.Asignada;
    }

    public void Iniciar()
    {
        Estado = EstadoSolicitud.EnProceso;
    }

    public void Resolver(string motivo, DateTime fechaResolucionUtc)
    {
        // RN-06: la resolución conserva su motivo y su fecha en UTC.
        Estado = EstadoSolicitud.Resuelta;
        MotivoResolucion = motivo;
        FechaResolucion = fechaResolucionUtc;
    }

    public void Cerrar()
    {
        Estado = EstadoSolicitud.Cerrada;
    }

    public void Reabrir()
    {
        Estado = EstadoSolicitud.EnProceso;
        FechaResolucion = null;
        MotivoResolucion = null;
    }

    public void Cancelar(string motivo)
    {
        // RN-06: la cancelación conserva su motivo por separado.
        Estado = EstadoSolicitud.Cancelada;
        MotivoCancelacion = motivo;
    }
}
