using Dominio.Enums;

namespace Dominio.Reglas;

/// Contiene las reglas de negocio para determinar los permisos
/// de los usuarios sobre las solicitudes según su rol.
public static class ReglasPermisoSolicitud
{
    /// RN-03: concentra los permisos de solicitudes por rol para que los
    /// servicios no repitan decisiones de autorización.

    public static bool PuedeVer(
        RolUsuario rol,
        Guid usuarioId,
        Guid solicitanteId)
    {
        return rol is RolUsuario.Admin or RolUsuario.Agente
            || usuarioId == solicitanteId;
    }

    public static bool PuedeEditar(
        RolUsuario rol,
        Guid usuarioId,
        Guid solicitanteId,
        EstadoSolicitud estado)
    {
        return rol is RolUsuario.Admin or RolUsuario.Agente
            || rol == RolUsuario.Solicitante
            && usuarioId == solicitanteId
            && estado == EstadoSolicitud.Nueva;
    }

    public static bool PuedeEjecutar(
        RolUsuario rol,
        Guid usuarioId,
        Guid solicitanteId,
        AccionSolicitud accion)
    {
        return accion switch
        {
            AccionSolicitud.Cancelar => rol == RolUsuario.Admin,
            AccionSolicitud.Cerrar => rol is RolUsuario.Admin
                or RolUsuario.Agente
                || rol == RolUsuario.Solicitante
                && usuarioId == solicitanteId,
            _ => rol is RolUsuario.Admin or RolUsuario.Agente
        };
    }
}
