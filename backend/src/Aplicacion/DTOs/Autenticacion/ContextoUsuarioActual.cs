using Dominio.Enums;

namespace Aplicacion.DTOs.Autenticacion;


// Contiene la información del usuario autenticado en la sesión.
public sealed record ContextoUsuarioActual(
    Guid UsuarioId,
    Guid TenantId,
    RolUsuario Rol,
    string Email);
