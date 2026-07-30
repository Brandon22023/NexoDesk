using Dominio.Enums;

namespace Dominio.Reglas;

public static class CalculadorSla
{
    public static DateTime CalcularFechaLimite(
        DateTime fechaCreacion,
        int slaHoras,
        PrioridadSolicitud prioridad)
    {
        if (slaHoras <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(slaHoras));
        }

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
        return fechaLimiteSla < fechaActualUtc
            && estado is not EstadoSolicitud.Resuelta
            and not EstadoSolicitud.Cerrada
            and not EstadoSolicitud.Cancelada;
    }
}
