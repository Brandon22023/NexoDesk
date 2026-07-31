using Dominio.Enums;

namespace Dominio.Entidades;


/// Entidad que representa un usuario del sistema, incluyendo
/// su información de autenticación y rol.
public sealed class Usuario
{
    private Usuario()
    {
    }
    
    // Inicializa un nuevo usuario.

    public Usuario(
        Guid id,
        Guid tenantId,
        string email,
        string passwordHash,
        string nombre,
        RolUsuario rol,
        bool activo = true)
    {
        Id = id;
        TenantId = tenantId;
        Email = email;
        PasswordHash = passwordHash;
        Nombre = nombre;
        Rol = rol;
        Activo = activo;
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public string Nombre { get; private set; } = string.Empty;

    public RolUsuario Rol { get; private set; }

    public bool Activo { get; private set; }

    public Tenant Tenant { get; private set; } = null!;

    public void ActualizarPasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash;
    }
}
