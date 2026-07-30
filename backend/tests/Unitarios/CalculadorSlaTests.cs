using Dominio.Enums;
using Dominio.Reglas;

namespace Unitarios;

public sealed class CalculadorSlaTests
{
    private static readonly DateTime FechaCreacion = new(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(PrioridadSolicitud.Critica, 4)]
    [InlineData(PrioridadSolicitud.Alta, 6)]
    [InlineData(PrioridadSolicitud.Media, 8)]
    [InlineData(PrioridadSolicitud.Baja, 16)]
    public void CalcularFechaLimite_AplicaElFactorDePrioridad(
        PrioridadSolicitud prioridad,
        int horasEsperadas)
    {
        var limite = CalculadorSla.CalcularFechaLimite(
            FechaCreacion,
            slaHoras: 8,
            prioridad);

        Assert.Equal(FechaCreacion.AddHours(horasEsperadas), limite);
    }

    [Fact]
    public void EstaVencida_CuandoLaSolicitudActivaSuperoElLimite_DevuelveVerdadero()
    {
        var vencida = CalculadorSla.EstaVencida(
            FechaCreacion.AddHours(-1),
            EstadoSolicitud.EnProceso,
            FechaCreacion);

        Assert.True(vencida);
    }

    [Theory]
    [InlineData(EstadoSolicitud.Resuelta)]
    [InlineData(EstadoSolicitud.Cerrada)]
    [InlineData(EstadoSolicitud.Cancelada)]
    public void EstaVencida_CuandoLaSolicitudEstaFinalizada_DevuelveFalso(
        EstadoSolicitud estado)
    {
        var vencida = CalculadorSla.EstaVencida(
            FechaCreacion.AddHours(-1),
            estado,
            FechaCreacion);

        Assert.False(vencida);
    }
}
