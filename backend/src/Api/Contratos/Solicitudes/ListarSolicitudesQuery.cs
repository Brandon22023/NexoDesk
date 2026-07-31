using Microsoft.AspNetCore.Mvc;

namespace Api.Contratos.Solicitudes;

// Parámetros de consulta para listar solicitudes:
// filtros, paginación y ordenamiento.
public sealed class ListarSolicitudesQuery
{   
    // Filtros opcionales
    [FromQuery(Name = "estado")]
    public string? Estado { get; init; }

    [FromQuery(Name = "prioridad")]
    public string? Prioridad { get; init; }

    [FromQuery(Name = "categoriaId")]
    public Guid? CategoriaId { get; init; }

    [FromQuery(Name = "agenteId")]
    public Guid? AgenteId { get; init; }

    [FromQuery(Name = "q")]
    public string? Q { get; init; }

    [FromQuery(Name = "vencidas")]
    public bool? Vencidas { get; init; }
    
    // Paginación
    [FromQuery(Name = "page")]
    public int Page { get; init; } = 1;

    [FromQuery(Name = "pageSize")]
    public int PageSize { get; init; } = 20;

    // Ordenamiento
    [FromQuery(Name = "sort")]
    public string Sort { get; init; } = "-fechaCreacion";
}
