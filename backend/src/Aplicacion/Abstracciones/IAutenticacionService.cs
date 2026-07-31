using Aplicacion.DTOs.Autenticacion;

namespace Aplicacion.Abstracciones;

// Define las operaciones necesarias para autenticar usuarios.
public interface IAutenticacionService
{
    // Valida credenciales y devuelve la información de sesión.
    Task<LoginResponse?> IniciarSesionAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);


    // Obtiene los datos del usuario autenticado.
    Task<UsuarioSesionDto?> ObtenerUsuarioAsync(
        Guid usuarioId,
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
