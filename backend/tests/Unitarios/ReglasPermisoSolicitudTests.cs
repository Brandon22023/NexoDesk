using Dominio.Enums;
using Dominio.Reglas;

namespace Unitarios;

// Pruebas de RN-03:
// validan que los permisos dependan del rol,
// propietario de la solicitud y estado actual.
public sealed class ReglasPermisoSolicitudTests
{

    // Identificadores fijos para simular usuarios distintos
    // sin depender de datos externos.
    private static readonly Guid UsuarioActualId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OtroSolicitanteId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Theory]
    [InlineData(RolUsuario.Admin, true)]
    [InlineData(RolUsuario.Agente, true)]
    [InlineData(RolUsuario.Solicitante, false)]
    public void PuedeVer_ConSolicitudDeOtroUsuario_RespetaElRol(
        RolUsuario rol,
        bool permitido)
    {
        // Comprueba que un solicitante no pueda acceder
        // a solicitudes creadas por otro usuario.
        var resultado = ReglasPermisoSolicitud.PuedeVer(
            rol,
            UsuarioActualId,
            OtroSolicitanteId);

        Assert.Equal(permitido, resultado);
    }

    [Fact]
    public void PuedeVer_SolicitanteConSolicitudPropia_DevuelveVerdadero()
    {
        // Un solicitante sí puede consultar sus propias solicitudes.
        var resultado = ReglasPermisoSolicitud.PuedeVer(
            RolUsuario.Solicitante,
            UsuarioActualId,
            UsuarioActualId);

        Assert.True(resultado);
    }

    [Theory]
    [InlineData(RolUsuario.Admin, EstadoSolicitud.Resuelta, true)]
    [InlineData(RolUsuario.Agente, EstadoSolicitud.EnProceso, true)]
    [InlineData(RolUsuario.Solicitante, EstadoSolicitud.Nueva, true)]
    [InlineData(RolUsuario.Solicitante, EstadoSolicitud.Asignada, false)]
    public void PuedeEditar_RespetaRolPropiedadYEstado(
        RolUsuario rol,
        EstadoSolicitud estado,
        bool permitido)
    {
        // Verifica que la edición dependa de tres factores:
        // rol del usuario, dueño de la solicitud y estado actual.
        var resultado = ReglasPermisoSolicitud.PuedeEditar(
            rol,
            UsuarioActualId,
            UsuarioActualId,
            estado);

        Assert.Equal(permitido, resultado);
    }

    [Fact]
    public void PuedeEditar_SolicitanteConSolicitudAjena_DevuelveFalso()
    {
        // Un solicitante no puede modificar solicitudes de otro usuario.
        var resultado = ReglasPermisoSolicitud.PuedeEditar(
            RolUsuario.Solicitante,
            UsuarioActualId,
            OtroSolicitanteId,
            EstadoSolicitud.Nueva);

        Assert.False(resultado);
    }

    [Theory]
    [InlineData(RolUsuario.Admin, AccionSolicitud.Cancelar, true)]
    [InlineData(RolUsuario.Agente, AccionSolicitud.Cancelar, false)]
    [InlineData(RolUsuario.Agente, AccionSolicitud.Resolver, true)]
    [InlineData(RolUsuario.Solicitante, AccionSolicitud.Iniciar, false)]
    public void PuedeEjecutar_RespetaLasAccionesPermitidasPorRol(
        RolUsuario rol,
        AccionSolicitud accion,
        bool permitido)
    {
        // Valida que cada rol pueda ejecutar únicamente
        // las acciones definidas en la regla de negocio.
        var resultado = ReglasPermisoSolicitud.PuedeEjecutar(
            rol,
            UsuarioActualId,
            UsuarioActualId,
            accion);

        Assert.Equal(permitido, resultado);
    }

    [Fact]
    public void PuedeEjecutar_SolicitanteSoloPuedeCerrarSolicitudPropia()
    {
        // Un solicitante puede cerrar únicamente solicitudes propias.
        var propia = ReglasPermisoSolicitud.PuedeEjecutar(
            RolUsuario.Solicitante,
            UsuarioActualId,
            UsuarioActualId,
            AccionSolicitud.Cerrar);
        var ajena = ReglasPermisoSolicitud.PuedeEjecutar(
            RolUsuario.Solicitante,
            UsuarioActualId,
            OtroSolicitanteId,
            AccionSolicitud.Cerrar);

        Assert.True(propia);
        Assert.False(ajena);
    }
}
