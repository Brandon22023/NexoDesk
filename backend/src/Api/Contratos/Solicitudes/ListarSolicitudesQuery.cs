namespace Api.Contratos.Solicitudes;

public sealed class ListarSolicitudesQuery
{
    public string? Estado { get; init; }

    public string? Prioridad { get; init; }

    public Guid? CategoriaId { get; init; }

    public Guid? AgenteId { get; init; }

    public string? Q { get; init; }

    public bool? Vencidas { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public string Sort { get; init; } = "-fechaCreacion";
}
