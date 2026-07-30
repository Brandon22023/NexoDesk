using System.Text;
using Api.Middleware;
using Api.Servicios;
using Aplicacion.Abstracciones;
using Infraestructura.Autenticacion;
using Infraestructura.Persistencia;
using Infraestructura.Persistencia.Seed;
using Infraestructura.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

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

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddScoped<IAutenticacionService, AutenticacionService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioActual, UsuarioActual>();
builder.Services.AddScoped<ISolicitudConsultaService, SolicitudConsultaService>();
builder.Services.AddScoped<ISolicitudService, SolicitudService>();
builder.Services.AddScoped<ICategoriaConsultaService, CategoriaConsultaService>();

var jwtSecret = builder.Configuration["Authentication:JwtSecret"] ?? string.Empty;
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MesaSitec API",
        Version = "v1",
        Description = "API de mesa de servicio multi-tenant."
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce el token JWT como: Bearer {token}"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        }] = Array.Empty<string>()
    });
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorExcepciones>();
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
    options.InvalidModelStateResponseFactory = context =>
    {
        var errores = context.ModelState
            .Where(item => item.Value?.Errors.Count > 0)
            .ToDictionary(
                item => item.Key,
                item => item.Value!.Errors
                    .Select(error => error.ErrorMessage)
                    .ToArray());
        var problem = new ValidationProblemDetails(errores)
        {
            Type = "https://mesasitec.local/errores/validacion",
            Title = "Error de validación",
            Status = StatusCodes.Status422UnprocessableEntity
        };
        problem.Extensions["codigo"] = "VALIDACION";
        return new UnprocessableEntityObjectResult(problem);
    });

var app = builder.Build();

await ApplyDatabaseChangesAsync(app, databasePath);

app.MapGet("/", () => "Hello World!");

app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "MesaSitec API v1");
    options.RoutePrefix = "swagger";
});
app.UseAuthentication();
app.UseAuthorization();
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
