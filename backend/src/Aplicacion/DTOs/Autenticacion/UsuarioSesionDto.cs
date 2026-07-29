namespace Aplicacion.DTOs.Autenticacion;

public sealed record UsuarioSesionDto(
    Guid Id,
    string Nombre,
    string Email,
    string Rol,
    Guid TenantId,
    string TenantNombre);
