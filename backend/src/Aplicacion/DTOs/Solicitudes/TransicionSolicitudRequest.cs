using System.ComponentModel.DataAnnotations;

namespace Aplicacion.DTOs.Solicitudes;

public sealed class TransicionSolicitudRequest
{
    [Required]
    public string Accion { get; init; } = string.Empty;

    public Guid? AgenteId { get; init; }

    public string? Motivo { get; init; }
}
