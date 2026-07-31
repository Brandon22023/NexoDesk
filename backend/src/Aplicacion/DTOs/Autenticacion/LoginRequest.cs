using System.ComponentModel.DataAnnotations;

namespace Aplicacion.DTOs.Autenticacion;

// Representa los datos enviados para iniciar sesión.
public sealed class LoginRequest
{
    // Correo del usuario.
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

// Contraseña del usuario.
    [Required]
    public string Password { get; init; } = string.Empty;
}
