using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class UsuarioConfiguracion : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> constructor)
    {
        constructor.HasKey(u => u.Id);
        constructor.Property(u => u.Username).HasMaxLength(50).IsRequired();
        constructor.Property(u => u.Email).HasMaxLength(150);
        constructor.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
        constructor.Property(u => u.NombreCompleto).HasMaxLength(200).IsRequired();
        constructor.Property(u => u.Activo).IsRequired();
        constructor.Property(u => u.UltimoAcceso).HasColumnType("timestamp with time zone");
        constructor.Property(u => u.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("NOW()").IsRequired();
        constructor.Property(u => u.UpdatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("NOW()").IsRequired();

        constructor.HasOne(u => u.Rol)
            .WithMany()
            .HasForeignKey(u => u.RolId)
            .OnDelete(DeleteBehavior.Restrict);

        constructor.HasIndex(u => u.Username).IsUnique();
        constructor.HasIndex(u => u.Email).IsUnique();
    }
}
