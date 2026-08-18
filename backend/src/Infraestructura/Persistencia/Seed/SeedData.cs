using System.Globalization;
using Dominio.Entidades;
using Dominio.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infraestructura.Persistencia.Seed;

/// Proporciona los datos iniciales para la base de datos.
public static class SeedData
{

    /// Inicializa la base de datos con los datos semilla.
private const string SeedPassword = "NexoDesk.2026";
    private const string DefaultBaseDate = "2026-01-15T08:00:00Z";

    public static async Task InitializeAsync(
        AppDbContext db,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(logger);

        logger.LogInformation("Iniciando la verificación de datos semilla.");

        try
        {
            if (await db.Tenants.AsNoTracking().AnyAsync())
            {
                logger.LogInformation(
                    "La base de datos ya contiene tenants. Se omite el seeding.");
                return;
            }

            var baseDate = ResolveBaseDate(logger);

            await using var transaction = await db.Database.BeginTransactionAsync();

            var tenants = CreateTenants();
            var users = CreateUsers(tenants);
            var categories = CreateCategories(tenants);
            var requests = CreateRequests(baseDate, tenants, users, categories);

            await db.Tenants.AddRangeAsync(tenants);
            await db.Usuarios.AddRangeAsync(users);
            await db.Categorias.AddRangeAsync(categories);
            await db.Solicitudes.AddRangeAsync(requests);

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            logger.LogInformation(
                "Seeding finalizado. Se crearon {TenantCount} tenants, " +
                "{UserCount} usuarios, {CategoryCount} categorías y " +
                "{RequestCount} solicitudes.",
                tenants.Count,
                users.Count,
                categories.Count,
                requests.Count);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Ocurrió un error durante el seeding.");
            throw;
        }
    }
    /// Obtiene la fecha base utilizada para generar los datos semilla.
    private static DateTime ResolveBaseDate(ILogger logger)
    {
        var configuredValue = Environment.GetEnvironmentVariable("SEED_FECHA_BASE");
        var value = string.IsNullOrWhiteSpace(configuredValue)
            ? DefaultBaseDate
            : configuredValue;

        if (DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var baseDate))
        {
            return DateTime.SpecifyKind(baseDate, DateTimeKind.Utc);
        }

        logger.LogWarning(
            "SEED_FECHA_BASE tiene un valor inválido. Se utilizará {DefaultBaseDate}.",
            DefaultBaseDate);

        return DateTime.Parse(
            DefaultBaseDate,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
    }
    
    /// Crea los tenants utilizados como datos semilla.
    private static List<Tenant> CreateTenants()
    {
        return
        [
            new(
                Guid.Parse("10000000-0000-0000-0000-000000000001"),
                "Cooperativa Norte"),
            new(
                Guid.Parse("10000000-0000-0000-0000-000000000002"),
                "Bufete Sur")
        ];
    }
    /// Crea los usuarios asociados a los tenants iniciales.
    private static List<Usuario> CreateUsers(IReadOnlyList<Tenant> tenants)
    {
        var northTenantId = tenants[0].Id;
        var southTenantId = tenants[1].Id;
        var passwordHasher = new PasswordHasher<Usuario>();

        var users = new List<Usuario>
        {
            CreateUser(
                "20000000-0000-0000-0000-000000000001",
                northTenantId,
                "admin@norte.test",
                "Administradora Norte",
                RolUsuario.Admin),
            CreateUser(
                "20000000-0000-0000-0000-000000000002",
                northTenantId,
                "agente1@norte.test",
                "Agente Uno Norte",
                RolUsuario.Agente),
            CreateUser(
                "20000000-0000-0000-0000-000000000003",
                northTenantId,
                "agente2@norte.test",
                "Agente Dos Norte",
                RolUsuario.Agente),
            CreateUser(
                "20000000-0000-0000-0000-000000000004",
                northTenantId,
                "user1@norte.test",
                "Solicitante Uno Norte",
                RolUsuario.Solicitante),
            CreateUser(
                "20000000-0000-0000-0000-000000000005",
                northTenantId,
                "user2@norte.test",
                "Solicitante Dos Norte",
                RolUsuario.Solicitante),
            CreateUser(
                "20000000-0000-0000-0000-000000000006",
                southTenantId,
                "admin@sur.test",
                "Administrador Sur",
                RolUsuario.Admin),
            CreateUser(
                "20000000-0000-0000-0000-000000000007",
                southTenantId,
                "user1@sur.test",
                "Solicitante Uno Sur",
                RolUsuario.Solicitante)
        };

        foreach (var user in users)
        {
            var passwordHash = passwordHasher.HashPassword(user, SeedPassword);
            user.ActualizarPasswordHash(passwordHash);
        }

        return users;
    }
    /// Crea un usuario con los datos indicados.
    private static Usuario CreateUser(
        string id,
        Guid tenantId,
        string email,
        string name,
        RolUsuario role)
    {
        return new Usuario(
            Guid.Parse(id),
            tenantId,
            email,
            string.Empty,
            name,
            role);
    }
    /// Crea las categorías asociadas a los tenants iniciales.
    private static List<Categoria> CreateCategories(
        IReadOnlyList<Tenant> tenants)
    {
        var categories = new List<Categoria>();

        for (var tenantIndex = 0; tenantIndex < tenants.Count; tenantIndex++)
        {
            var tenantId = tenants[tenantIndex].Id;
            var idOffset = tenantIndex * 4;

            categories.AddRange(
            [
                new(
                    CreateCategoryId(idOffset + 1),
                    tenantId,
                    "Incidente",
                    8),
                new(
                    CreateCategoryId(idOffset + 2),
                    tenantId,
                    "Requerimiento",
                    40),
                new(
                    CreateCategoryId(idOffset + 3),
                    tenantId,
                    "Consulta",
                    24),
                new(
                    CreateCategoryId(idOffset + 4),
                    tenantId,
                    "Falla crítica",
                    4)
            ]);
        }

        return categories;
    }
    /// Crea las solicitudes asociadas a los tenants iniciales.
    private static List<Solicitud> CreateRequests(
        DateTime baseDate,
        IReadOnlyList<Tenant> tenants,
        IReadOnlyList<Usuario> users,
        IReadOnlyList<Categoria> categories)
    {
        var requests = new List<Solicitud>();

        AddTenantRequests(
            requests,
            tenant: tenants[0],
            tenantCategories: categories
                .Where(category => category.TenantId == tenants[0].Id)
                .ToArray(),
            requesters:
            [
                users.Single(user => user.Email == "user1@norte.test"),
                users.Single(user => user.Email == "user2@norte.test")
            ],
            agents:
            [
                users.Single(user => user.Email == "agente1@norte.test"),
                users.Single(user => user.Email == "agente2@norte.test")
            ],
            count: 25,
            baseDate: baseDate.AddHours(-96));

        AddTenantRequests(
            requests,
            tenant: tenants[1],
            tenantCategories: categories
                .Where(category => category.TenantId == tenants[1].Id)
                .ToArray(),
            requesters:
            [
                users.Single(user => user.Email == "user1@sur.test")
            ],
            agents:
            [
                users.Single(user => user.Email == "admin@sur.test")
            ],
            count: 8,
            baseDate: baseDate.AddHours(-48));

        return requests;
    }
    /// Agrega solicitudes de prueba para un tenant.
    private static void AddTenantRequests(
        ICollection<Solicitud> requests,
        Tenant tenant,
        IReadOnlyList<Categoria> tenantCategories,
        IReadOnlyList<Usuario> requesters,
        IReadOnlyList<Usuario> agents,
        int count,
        DateTime baseDate)
    {
        EstadoSolicitud[] states =
        [
            EstadoSolicitud.Nueva,
            EstadoSolicitud.Asignada,
            EstadoSolicitud.EnProceso,
            EstadoSolicitud.Resuelta,
            EstadoSolicitud.Cerrada,
            EstadoSolicitud.Cancelada
        ];

        PrioridadSolicitud[] priorities =
        [
            PrioridadSolicitud.Critica,
            PrioridadSolicitud.Alta,
            PrioridadSolicitud.Media,
            PrioridadSolicitud.Baja
        ];

        for (var index = 0; index < count; index++)
        {
            var sequence = index + 1;
            var category = tenantCategories[index % tenantCategories.Count];
            var requester = requesters[index % requesters.Count];
            var state = states[index % states.Length];
            var priority = priorities[index % priorities.Length];
            var creationDate = baseDate.AddHours(index * 3);
            var deadline = creationDate.AddHours(
                category.SlaHoras * GetSlaFactor(priority));
            var isResolved = state is EstadoSolicitud.Resuelta
                or EstadoSolicitud.Cerrada;
            var needsAgent = state is EstadoSolicitud.Asignada
                or EstadoSolicitud.EnProceso
                or EstadoSolicitud.Resuelta
                or EstadoSolicitud.Cerrada;
            Guid? agentId = needsAgent
                ? agents[index % agents.Count].Id
                : null;

            requests.Add(
                new Solicitud(
                    CreateRequestId(tenant.Id, sequence),
                    tenant.Id,
                    $"SOL-{creationDate.Year}-{sequence:00000}",
                    $"Solicitud de soporte número {sequence}",
                    $"Descripción detallada de la solicitud de soporte número {sequence}.",
                    category.Id,
                    priority,
                    state,
                    requester.Id,
                    agentId,
                    creationDate,
                    deadline,
                    isResolved ? creationDate.AddHours(2) : null,
                    isResolved
                        ? "La solicitud fue atendida y validada correctamente."
                        : null,
                    state == EstadoSolicitud.Cancelada
                        ? "Solicitud duplicada y cancelada por administración."
                        : null));
        }
    }
    /// Obtiene el factor de SLA correspondiente a una prioridad.
    private static double GetSlaFactor(PrioridadSolicitud priority)
    {
        return priority switch
        {
            PrioridadSolicitud.Critica => 0.5,
            PrioridadSolicitud.Alta => 0.75,
            PrioridadSolicitud.Media => 1.0,
            PrioridadSolicitud.Baja => 2.0,
            _ => throw new ArgumentOutOfRangeException(
                nameof(priority),
                priority,
                "Prioridad no soportada.")
        };
    }
    /// Genera el identificador de una categoría.
    private static Guid CreateCategoryId(int sequence)
    {
        return Guid.Parse($"30000000-0000-0000-0000-{sequence:000000000000}");
    }
    /// Genera el identificador de una solicitud.
    private static Guid CreateRequestId(Guid tenantId, int sequence)
    {
        var bytes = tenantId.ToByteArray();
        BitConverter.GetBytes(sequence).CopyTo(bytes, 8);
        return new Guid(bytes);
    }
}
