using Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructura.Persistencia.Configuraciones;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(tenant => tenant.Id);

        builder.Property(tenant => tenant.Id)
            .ValueGeneratedNever();

        builder.Property(tenant => tenant.Nombre)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(tenant => tenant.Activo)
            .IsRequired();

        builder.HasIndex(tenant => tenant.Nombre)
            .IsUnique()
            .HasDatabaseName("UX_Tenants_Nombre");

        builder.HasIndex(tenant => tenant.Activo)
            .HasDatabaseName("IX_Tenants_Activo");
    }
}
