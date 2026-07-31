using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infraestructura.Persistencia;

/// Crea instancias de <see cref="AppDbContext"/> en tiempo de diseño.
public sealed class DesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<AppDbContext>
{
    /// Crea una instancia del contexto para herramientas de Entity Framework Core.
    public AppDbContext CreateDbContext(string[] args)
    {
        var databasePath = DatabasePathResolver.Resolve(Directory.GetCurrentDirectory());
        DatabasePathResolver.EnsureDirectoryExists(databasePath);

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            ForeignKeys = true
        }.ToString();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(
                connectionString,
                sqliteOptions => sqliteOptions.MigrationsAssembly(
                    typeof(AppDbContext).Assembly.FullName))
            .Options;

        return new AppDbContext(options);
    }
}
