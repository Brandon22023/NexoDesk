using System.Text;
using Api.Middleware;
using Api.Servicios;
using Api.Swagger;
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
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
const string FrontendCorsPolicy = "Frontend";

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();


// La base de datos SQLite se resuelve según el entorno.
// En Docker utiliza el volumen configurado para conservar los datos.
var databasePath = DatabasePathResolver.Resolve(
    builder.Environment.ContentRootPath);
DatabasePathResolver.EnsureDirectoryExists(databasePath);

var connectionString = new SqliteConnectionStringBuilder
{
    DataSource = databasePath,
    ForeignKeys = true
}.ToString();


// EF Core administra la persistencia mediante SQLite y aplica
// migraciones automáticamente al iniciar la aplicación.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        connectionString,
        sqliteOptions => sqliteOptions.MigrationsAssembly(
            typeof(AppDbContext).Assembly.FullName)));

// Registro de servicios de aplicación.
// Los controllers consumen abstracciones y no implementaciones concretas.

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddScoped<IAutenticacionService, AutenticacionService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioActual, UsuarioActual>();
builder.Services.AddScoped<ISolicitudConsultaService, SolicitudConsultaService>();
builder.Services.AddScoped<ISolicitudService, SolicitudService>();
builder.Services.AddScoped<ICategoriaConsultaService, CategoriaConsultaService>();


// Configuración de autenticación mediante JWT.
// El token contiene la identidad del usuario y el tenant al que pertenece.
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

// CORS permite que el frontend pueda comunicarse con la API
// únicamente desde los orígenes configurados.
var corsAllowedOrigins = (builder.Configuration["Cors:AllowedOrigins"]
        ?? string.Empty)
    .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

if (corsAllowedOrigins.Length == 0)
{
    throw new InvalidOperationException(
        "Cors:AllowedOrigins debe incluir al menos un origen permitido.");
}


// Configuración de Swagger con autenticación Bearer
// para poder probar endpoints protegidos con JWT.
builder.Services.AddCors(options =>
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins(corsAllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
Title = "NexoDesk API",
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
    options.OperationFilter<PublicEndpointOperationFilter>();
    options.OperationFilter<ErrorResponseExamplesOperationFilter>();
    options.SchemaFilter<ProblemDetailsSchemaFilter>();

    var apiXml = Path.Combine(AppContext.BaseDirectory,
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    options.IncludeXmlComments(apiXml, includeControllerXmlComments: true);
});

// Manejo centralizado de errores para evitar respuestas con stack trace
// y mantener un formato uniforme para la API.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorExcepciones>();
builder.Services.AddControllers();

// Personalización de errores de validación para cumplir el contrato
// de respuestas definido por la aplicación.
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
Type = "https://nexodesk.local/errores/validacion",
            Title = "Error de validación",
            Status = StatusCodes.Status422UnprocessableEntity
        };
        problem.Extensions["codigo"] = "VALIDACION";
        return new UnprocessableEntityObjectResult(problem);
    });

var app = builder.Build();
// Al iniciar la aplicación se crean las tablas pendientes
// y se cargan los datos iniciales necesarios para el funcionamiento de la aplicación.
await ApplyDatabaseChangesAsync(app, databasePath);

app.MapGet("/", () => "Hello World!").ExcludeFromDescription();

app.UseExceptionHandler();
app.UseCors(FrontendCorsPolicy);
app.UseSwagger();
app.UseSwaggerUI(options =>
{
options.SwaggerEndpoint("/swagger/v1/swagger.json", "NexoDesk API v1");
    options.RoutePrefix = "swagger";
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
await app.RunAsync();

// Ejecuta migraciones y seed de forma automática.
// Esto permite levantar el proyecto sin pasos manuales adicionales.
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
