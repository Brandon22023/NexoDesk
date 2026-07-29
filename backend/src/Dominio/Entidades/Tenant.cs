namespace Dominio.Entidades;

public sealed class Tenant
{
    private Tenant()
    {
    }

    public Tenant(Guid id, string nombre, bool activo = true)
    {
        Id = id;
        Nombre = nombre;
        Activo = activo;
    }

    public Guid Id { get; private set; }

    public string Nombre { get; private set; } = string.Empty;

    public bool Activo { get; private set; }
}
