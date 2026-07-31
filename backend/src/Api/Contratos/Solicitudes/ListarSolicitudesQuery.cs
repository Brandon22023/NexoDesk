namespace Api.Contratos.Solicitudes;

// Parámetros de consulta para listar solicitudes:
// filtros, paginación y ordenamiento.
public sealed class ListarSolicitudesQuery
{   
    // Filtros opcionales
    public string? Estado { get; init; }

    public string? Prioridad { get; init; }

    public Guid? CategoriaId { get; init; }

    public Guid? AgenteId { get; init; }

    public string? Q { get; init; }

    public bool? Vencidas { get; init; }
    
    // Paginación
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    // Ordenamiento
    public string Sort { get; init; } = "-fechaCreacion";
}
