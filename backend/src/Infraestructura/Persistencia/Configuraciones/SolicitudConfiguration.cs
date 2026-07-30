using Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructura.Persistencia.Configuraciones;

public sealed class SolicitudConfiguration : IEntityTypeConfiguration<Solicitud>
{
    public void Configure(EntityTypeBuilder<Solicitud> builder)
    {
        builder.ToTable(
            "Solicitudes",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_Solicitudes_Titulo_Longitud",
                    "length(\"Titulo\") BETWEEN 5 AND 120");
                tableBuilder.HasCheckConstraint(
                    "CK_Solicitudes_Descripcion_Longitud",
                    "length(\"Descripcion\") BETWEEN 10 AND 4000");
            });

        builder.HasKey(solicitud => solicitud.Id);

        builder.Property(solicitud => solicitud.Id)
            .ValueGeneratedNever();

        builder.Property(solicitud => solicitud.TenantId)
            .IsRequired();

        builder.Property(solicitud => solicitud.Codigo)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(solicitud => solicitud.Titulo)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(solicitud => solicitud.Descripcion)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(solicitud => solicitud.CategoriaId)
            .IsRequired();

        builder.Property(solicitud => solicitud.Prioridad)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(solicitud => solicitud.Estado)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(solicitud => solicitud.SolicitanteId)
            .IsRequired();

        builder.Property(solicitud => solicitud.FechaCreacion)
            .IsRequired();

        builder.Property(solicitud => solicitud.FechaLimiteSla)
            .IsRequired();

        builder.Property(solicitud => solicitud.MotivoResolucion)
            .HasMaxLength(4000);

        builder.Property(solicitud => solicitud.MotivoCancelacion)
            .HasMaxLength(4000);

        builder.HasOne(solicitud => solicitud.Tenant)
            .WithMany()
            .HasForeignKey(solicitud => solicitud.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(solicitud => solicitud.Categoria)
            .WithMany()
            .HasForeignKey(solicitud => solicitud.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(solicitud => solicitud.Solicitante)
            .WithMany()
            .HasForeignKey(solicitud => solicitud.SolicitanteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(solicitud => solicitud.Agente)
            .WithMany()
            .HasForeignKey(solicitud => solicitud.AgenteId)
            .OnDelete(DeleteBehavior.Restrict);

        // RN-07: el mismo código solo puede existir una vez por organización.
        builder.HasIndex(solicitud => new { solicitud.TenantId, solicitud.Codigo })
            .IsUnique()
            .HasDatabaseName("UX_Solicitudes_TenantId_Codigo");

        builder.HasIndex(solicitud => new
            {
                solicitud.TenantId,
                solicitud.Estado,
                solicitud.FechaCreacion
            })
            .HasDatabaseName("IX_Solicitudes_TenantId_Estado_FechaCreacion");

        builder.HasIndex(solicitud => new
            {
                solicitud.TenantId,
                solicitud.Prioridad,
                solicitud.FechaCreacion
            })
            .HasDatabaseName("IX_Solicitudes_TenantId_Prioridad_FechaCreacion");

        builder.HasIndex(solicitud => new { solicitud.TenantId, solicitud.CategoriaId })
            .HasDatabaseName("IX_Solicitudes_TenantId_CategoriaId");

        builder.HasIndex(solicitud => new { solicitud.TenantId, solicitud.AgenteId })
            .HasDatabaseName("IX_Solicitudes_TenantId_AgenteId");

        builder.HasIndex(solicitud => new { solicitud.TenantId, solicitud.SolicitanteId })
            .HasDatabaseName("IX_Solicitudes_TenantId_SolicitanteId");

        builder.HasIndex(solicitud => new
            {
                solicitud.TenantId,
                solicitud.FechaLimiteSla
            })
            .HasDatabaseName("IX_Solicitudes_TenantId_FechaLimiteSla");
    }
}
