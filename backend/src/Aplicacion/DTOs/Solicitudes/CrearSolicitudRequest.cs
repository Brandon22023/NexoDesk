using System.ComponentModel.DataAnnotations;

namespace Aplicacion.DTOs.Solicitudes;

public sealed class CrearSolicitudRequest
{
    [Required, StringLength(120, MinimumLength = 5)]
    public string Titulo { get; init; } = string.Empty;

    [Required, StringLength(4000, MinimumLength = 10)]
    public string Descripcion { get; init; } = string.Empty;

    [Required]
    public Guid? CategoriaId { get; init; }

    [Required]
    public string Prioridad { get; init; } = string.Empty;
}
