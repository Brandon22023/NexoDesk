using System.Security.Claims;
using Aplicacion.Abstracciones;
using Aplicacion.DTOs.Autenticacion;
using Dominio.Enums;

namespace Api.Servicios;
// Obtiene la información del usuario autenticado
// a partir de los claims incluidos en el token JWT.
public sealed class UsuarioActual(IHttpContextAccessor httpContextAccessor)
    : IUsuarioActual
{
    public ContextoUsuarioActual? Obtener()
    {
        var principal = httpContextAccessor.HttpContext?.User;
        
        // Verifica que exista un usuario autenticado y que los
        // claims obligatorios del sistema sean válidos.
        if (principal?.Identity?.IsAuthenticated != true
            || !Guid.TryParse(
                principal.FindFirstValue("sub"),
                out var usuarioId)
            || !Guid.TryParse(
                principal.FindFirstValue("tenantId"),
                out var tenantId)
            || !Enum.TryParse<RolUsuario>(
                principal.FindFirstValue("rol"),
                ignoreCase: true,
                out var rol))
        {
            return null;
        }

        // Construye un objeto de contexto reutilizable para que
        // la aplicación conozca quién realiza la operación.
        return new ContextoUsuarioActual(
            usuarioId,
            tenantId,
            rol,
            principal.FindFirstValue("email") ?? string.Empty);
    }
}
