namespace Aplicacion.DTOs.Autenticacion;

// Contiene la respuesta generada después de un inicio de sesión exitoso.
public sealed record LoginResponse(
    string AccessToken,
    int ExpiraEn,
    UsuarioSesionDto Usuario);
