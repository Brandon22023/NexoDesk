using System.Security.Claims;
using Aplicacion.Abstracciones;
using Aplicacion.DTOs.Autenticacion;
using Dominio.Enums;

namespace Api.Servicios;

public sealed class UsuarioActual(IHttpContextAccessor httpContextAccessor)
    : IUsuarioActual
{
    public ContextoUsuarioActual? Obtener()
    {
        var principal = httpContextAccessor.HttpContext?.User;

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

        return new ContextoUsuarioActual(
            usuarioId,
            tenantId,
            rol,
            principal.FindFirstValue("email") ?? string.Empty);
    }
}
