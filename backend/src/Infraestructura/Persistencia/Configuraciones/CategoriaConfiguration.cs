using Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructura.Persistencia.Configuraciones;

public sealed class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable(
            "Categorias",
            tableBuilder => tableBuilder.HasCheckConstraint(
                "CK_Categorias_SlaHoras_Positivo",
                "\"SlaHoras\" > 0"));

        builder.HasKey(categoria => categoria.Id);

        builder.Property(categoria => categoria.Id)
            .ValueGeneratedNever();

        builder.Property(categoria => categoria.TenantId)
            .IsRequired();

        builder.Property(categoria => categoria.Nombre)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(categoria => categoria.SlaHoras)
            .IsRequired();

        builder.Property(categoria => categoria.Activo)
            .IsRequired();

        builder.HasOne(categoria => categoria.Tenant)
            .WithMany()
            .HasForeignKey(categoria => categoria.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(categoria => new { categoria.TenantId, categoria.Nombre })
            .IsUnique()
            .HasDatabaseName("UX_Categorias_TenantId_Nombre");

        builder.HasIndex(categoria => new { categoria.TenantId, categoria.Activo })
            .HasDatabaseName("IX_Categorias_TenantId_Activo");
    }
}
