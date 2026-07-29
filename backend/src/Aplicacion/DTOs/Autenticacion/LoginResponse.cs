namespace Aplicacion.DTOs.Autenticacion;

public sealed record LoginResponse(
    string AccessToken,
    int ExpiraEn,
    UsuarioSesionDto Usuario);
