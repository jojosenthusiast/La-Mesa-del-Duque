using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class RolConfiguracion : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> constructor)
    {
        constructor.HasKey(r => r.Id);
        constructor.Property(r => r.Nombre).HasMaxLength(50).IsRequired();
        constructor.Property(r => r.Descripcion).HasMaxLength(250);
        constructor.Property(r => r.Activo).IsRequired();
        constructor.Property(r => r.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("NOW()").IsRequired();
        constructor.HasIndex(r => r.Nombre).IsUnique();
    }
}
