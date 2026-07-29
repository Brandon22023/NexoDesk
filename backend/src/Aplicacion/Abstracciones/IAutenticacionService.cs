using Aplicacion.DTOs.Autenticacion;

namespace Aplicacion.Abstracciones;

public interface IAutenticacionService
{
    Task<LoginResponse?> IniciarSesionAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<UsuarioSesionDto?> ObtenerUsuarioAsync(
        Guid usuarioId,
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
