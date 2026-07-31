namespace Aplicacion.DTOs.Solicitudes;
/// DTO utilizado para representar una referencia básica de una solicitud
/// mediante su identificador y nombre.
public sealed record ReferenciaSolicitudDto(Guid Id, string Nombre);
