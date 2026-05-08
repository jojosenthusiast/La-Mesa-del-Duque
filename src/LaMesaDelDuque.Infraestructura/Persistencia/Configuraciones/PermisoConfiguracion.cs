using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class PermisoConfiguracion : IEntityTypeConfiguration<Permiso>
{
    public void Configure(EntityTypeBuilder<Permiso> constructor)
    {
        constructor.HasKey(p => p.Id);
        constructor.Property(p => p.Nombre).HasMaxLength(100).IsRequired();
        constructor.Property(p => p.Modulo).HasMaxLength(50).IsRequired();
        constructor.Property(p => p.Descripcion).HasMaxLength(250);
        constructor.HasIndex(p => p.Nombre).IsUnique();
    }
}
