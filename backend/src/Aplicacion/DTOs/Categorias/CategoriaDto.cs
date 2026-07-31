namespace Aplicacion.DTOs.Categorias;

// Información de una categoría disponible para las solicitudes.
public sealed record CategoriaDto(
    Guid Id,
    string Nombre,
    int SlaHoras);
