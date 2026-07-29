using Infraestructura.Persistencia;
using Infraestructura.Persistencia.Seed;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var databasePath = DatabasePathResolver.Resolve(
    builder.Environment.ContentRootPath);
DatabasePathResolver.EnsureDirectoryExists(databasePath);

var connectionString = new SqliteConnectionStringBuilder
{
    DataSource = databasePath,
    ForeignKeys = true
}.ToString();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        connectionString,
        sqliteOptions => sqliteOptions.MigrationsAssembly(
            typeof(AppDbContext).Assembly.FullName)));

builder.Services.AddControllers();

var app = builder.Build();

await ApplyDatabaseChangesAsync(app, databasePath);

app.MapGet("/", () => "Hello World!");

app.MapControllers();
await app.RunAsync();

static async Task ApplyDatabaseChangesAsync(
    WebApplication app,
    string databasePath)
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("Infraestructura.Persistencia");

    try
    {
        logger.LogInformation(
            "Iniciando migraciones de la base de datos SQLite en {DatabasePath}.",
            databasePath);

        await db.Database.MigrateAsync();

        logger.LogInformation(
            "Migraciones de la base de datos finalizadas correctamente.");

        logger.LogInformation("Iniciando seeding de la base de datos.");
        await SeedData.InitializeAsync(db, logger);
        logger.LogInformation("Seeding de la base de datos finalizado.");
    }
    catch (Exception exception)
    {
        logger.LogCritical(
            exception,
            "No fue posible inicializar la base de datos.");
        throw;
    }
}
