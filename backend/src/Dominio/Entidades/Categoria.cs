namespace Dominio.Entidades;

/// Entidad que representa una categoría de solicitudes,
/// incluyendo el tiempo de SLA configurado.
public sealed class Categoria
{
    private Categoria()
    {
    }

    public Categoria(
        Guid id,
        Guid tenantId,
        string nombre,
        int slaHoras,
        bool activo = true)
    {
        Id = id;
        TenantId = tenantId;
        Nombre = nombre;
        SlaHoras = slaHoras;
        Activo = activo;
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string Nombre { get; private set; } = string.Empty;

    public int SlaHoras { get; private set; }

    public bool Activo { get; private set; }

    public Tenant Tenant { get; private set; } = null!;
}
