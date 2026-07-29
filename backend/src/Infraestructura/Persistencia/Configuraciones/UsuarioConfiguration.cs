using Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructura.Persistencia.Configuraciones;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(usuario => usuario.Id);

        builder.Property(usuario => usuario.Id)
            .ValueGeneratedNever();

        builder.Property(usuario => usuario.TenantId)
            .IsRequired();

        builder.Property(usuario => usuario.Email)
            .HasMaxLength(254)
            .UseCollation("NOCASE")
            .IsRequired();

        builder.Property(usuario => usuario.PasswordHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(usuario => usuario.Nombre)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(usuario => usuario.Rol)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(usuario => usuario.Activo)
            .IsRequired();

        builder.HasOne(usuario => usuario.Tenant)
            .WithMany()
            .HasForeignKey(usuario => usuario.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(usuario => usuario.Email)
            .IsUnique()
            .HasDatabaseName("UX_Usuarios_Email");

        builder.HasIndex(usuario => new { usuario.TenantId, usuario.Activo })
            .HasDatabaseName("IX_Usuarios_TenantId_Activo");

        builder.HasIndex(usuario => new { usuario.TenantId, usuario.Rol })
            .HasDatabaseName("IX_Usuarios_TenantId_Rol");
    }
}
