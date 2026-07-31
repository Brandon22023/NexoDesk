namespace Aplicacion.DTOs.Autenticacion;


// Contiene la información básica del usuario autenticado obtenida desde el token JWT.
public sealed record UsuarioSesionDto(
    Guid Id,
    string Nombre,
    string Email,
    string Rol,
    Guid TenantId,
    string TenantNombre);
