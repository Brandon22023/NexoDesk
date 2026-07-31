using System.ComponentModel.DataAnnotations;

namespace Aplicacion.DTOs.Solicitudes;

/// DTO utilizado para solicitar un cambio de estado o transición
/// de una solicitud.
public sealed class TransicionSolicitudRequest
{
    [Required]
    public string Accion { get; init; } = string.Empty;

    public Guid? AgenteId { get; init; }

    public string? Motivo { get; init; }
}
