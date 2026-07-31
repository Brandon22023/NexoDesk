using Dominio.Enums;
using Dominio.Reglas;

namespace Unitarios;

// Pruebas unitarias para validar la máquina de estados de las solicitudes.
// Verifica que solamente se permitan las transiciones definidas por las reglas de negocio.
public sealed class ReglasTransicionSolicitudTests
{
    // Valida que las combinaciones de estado actual + acción sean aceptadas
    // cuando existen dentro del flujo permitido del sistema.
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
        // Si la transición existe dentro de las reglas del negocio,
        // el sistema debe permitir ejecutarla.
        Assert.True(ReglasTransicionSolicitud.EsPermitida(estado, accion));
    }
    // Valida que acciones no contempladas en el flujo de estados
    // sean rechazadas para evitar cambios inválidos.

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
        // Una transición no definida debe ser bloqueada,
        // evitando que una solicitud pase a estados no permitidos.
        Assert.False(ReglasTransicionSolicitud.EsPermitida(estado, accion));
    }
}
