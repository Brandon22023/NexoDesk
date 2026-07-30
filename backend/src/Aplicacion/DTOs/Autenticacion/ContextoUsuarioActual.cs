using Dominio.Enums;

namespace Aplicacion.DTOs.Autenticacion;

public sealed record ContextoUsuarioActual(
    Guid UsuarioId,
    Guid TenantId,
    RolUsuario Rol,
    string Email);
