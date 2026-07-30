using Dominio.Enums;

namespace Dominio.Reglas;

public static class CalculadorSla
{

    /// RN-04: calcula el SLA en el servidor usando las horas de la categoría
    /// y el factor definido para la prioridad.

    public static DateTime CalcularFechaLimite(
        DateTime fechaCreacion,
        int slaHoras,
        PrioridadSolicitud prioridad)
    {
        if (slaHoras <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(slaHoras));
        }

        // Critica 0.5, Alta 0.75, Media 1.0 y Baja 2.0.
        var factor = prioridad switch
        {
            PrioridadSolicitud.Critica => 0.5,
            PrioridadSolicitud.Alta => 0.75,
            PrioridadSolicitud.Media => 1.0,
            PrioridadSolicitud.Baja => 2.0,
            _ => throw new ArgumentOutOfRangeException(nameof(prioridad))
        };

        return fechaCreacion.AddHours(slaHoras * factor);
    }

    public static bool EstaVencida(
        DateTime fechaLimiteSla,
        EstadoSolicitud estado,
        DateTime fechaActualUtc)
    {
        // RN-04: una solicitud finalizada nunca se considera vencida.
        return fechaLimiteSla < fechaActualUtc
            && estado is not EstadoSolicitud.Resuelta
            and not EstadoSolicitud.Cerrada
            and not EstadoSolicitud.Cancelada;
    }
}
