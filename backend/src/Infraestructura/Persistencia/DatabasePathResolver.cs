namespace Infraestructura.Persistencia;

/// Proporciona utilidades para resolver la ubicación de la base de datos.
public static class DatabasePathResolver
{
    private const string DatabaseFileName = "mesasitec.db";

    /// Obtiene la ruta de la base de datos a partir de un directorio inicial.
    public static string Resolve(string startPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(startPath);

        var current = new DirectoryInfo(Path.GetFullPath(startPath));

        while (current is not null)
        {
            if (current.Name.Equals("backend", StringComparison.OrdinalIgnoreCase))
            {
                return BuildDatabasePath(current.FullName);
            }

            var backendCandidate = Path.Combine(current.FullName, "backend");
            if (Directory.Exists(Path.Combine(backendCandidate, "src", "Api")))
            {
                return BuildDatabasePath(backendCandidate);
            }

            current = current.Parent;
        }

        return BuildDatabasePath(Path.GetFullPath(startPath));
    }
    /// Garantiza que exista el directorio donde se almacenará la base de datos.
    public static void EnsureDirectoryExists(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        var dataDirectory = Path.GetDirectoryName(databasePath)
            ?? throw new InvalidOperationException(
                "No se pudo determinar la carpeta de la base de datos.");

        Directory.CreateDirectory(dataDirectory);
    }
    /// Construye la ruta completa de la base de datos.
    private static string BuildDatabasePath(string backendRoot)
    {
        return Path.Combine(backendRoot, "data", DatabaseFileName);
    }
}
