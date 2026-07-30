namespace Aplicacion.DTOs.Categorias;

public sealed record CategoriaDto(
    Guid Id,
    string Nombre,
    int SlaHoras);
