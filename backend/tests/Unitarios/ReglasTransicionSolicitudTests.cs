using Dominio.Enums;
using Dominio.Reglas;

namespace Unitarios;

public sealed class ReglasTransicionSolicitudTests
{
    [Theory]
    [InlineData(EstadoSolicitud.Nueva, AccionSolicitud.Asignar)]
    [InlineData(EstadoSolicitud.Nueva, AccionSolicitud.Cancelar)]
    [InlineData(EstadoSolicitud.Asignada, AccionSolicitud.Iniciar)]
    [InlineData(EstadoSolicitud.Asignada, AccionSolicitud.Asignar)]
    [InlineData(EstadoSolicitud.Asignada, AccionSolicitud.Cancelar)]
    [InlineData(EstadoSolicitud.EnProceso, AccionSolicitud.Resolver)]
    [InlineData(EstadoSolicitud.EnProceso, AccionSolicitud.Asignar)]
    [InlineData(EstadoSolicitud.EnProceso, AccionSolicitud.Cancelar)]
    [InlineData(EstadoSolicitud.Resuelta, AccionSolicitud.Cerrar)]
    [InlineData(EstadoSolicitud.Resuelta, AccionSolicitud.Reabrir)]
    public void EsPermitida_CuandoLaTransicionEstaDefinida_DevuelveVerdadero(
        EstadoSolicitud estado,
        AccionSolicitud accion)
    {
        Assert.True(ReglasTransicionSolicitud.EsPermitida(estado, accion));
    }

    [Theory]
    [InlineData(EstadoSolicitud.Nueva, AccionSolicitud.Resolver)]
    [InlineData(EstadoSolicitud.Asignada, AccionSolicitud.Cerrar)]
    [InlineData(EstadoSolicitud.EnProceso, AccionSolicitud.Iniciar)]
    [InlineData(EstadoSolicitud.Resuelta, AccionSolicitud.Cancelar)]
    [InlineData(EstadoSolicitud.Cerrada, AccionSolicitud.Cerrar)]
    [InlineData(EstadoSolicitud.Cancelada, AccionSolicitud.Reabrir)]
    public void EsPermitida_CuandoLaTransicionNoEstaDefinida_DevuelveFalso(
        EstadoSolicitud estado,
        AccionSolicitud accion)
    {
        Assert.False(ReglasTransicionSolicitud.EsPermitida(estado, accion));
    }
}
