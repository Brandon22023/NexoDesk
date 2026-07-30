using Dominio.Enums;

namespace Dominio.Reglas;

public static class ReglasTransicionSolicitud
{

    /// RN-02: define las únicas transiciones válidas por estado.
    /// Cerrada y Cancelada son estados finales porque no tienen casos
    /// permitidos; cualquier combinación no listada devuelve false.

    public static bool EsPermitida(
        EstadoSolicitud estado,
        AccionSolicitud accion)
    {
        return (estado, accion) switch
        {
            (EstadoSolicitud.Nueva, AccionSolicitud.Asignar) => true,
            (EstadoSolicitud.Nueva, AccionSolicitud.Cancelar) => true,
            (EstadoSolicitud.Asignada, AccionSolicitud.Iniciar) => true,
            (EstadoSolicitud.Asignada, AccionSolicitud.Asignar) => true,
            (EstadoSolicitud.Asignada, AccionSolicitud.Cancelar) => true,
            (EstadoSolicitud.EnProceso, AccionSolicitud.Resolver) => true,
            (EstadoSolicitud.EnProceso, AccionSolicitud.Asignar) => true,
            (EstadoSolicitud.EnProceso, AccionSolicitud.Cancelar) => true,
            (EstadoSolicitud.Resuelta, AccionSolicitud.Cerrar) => true,
            (EstadoSolicitud.Resuelta, AccionSolicitud.Reabrir) => true,
            _ => false
        };
    }
}
