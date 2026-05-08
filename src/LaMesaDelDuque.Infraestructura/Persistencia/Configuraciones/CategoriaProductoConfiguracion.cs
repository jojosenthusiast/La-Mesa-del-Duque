using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class CategoriaProductoConfiguracion : IEntityTypeConfiguration<CategoriaProducto>
{
    public void Configure(EntityTypeBuilder<CategoriaProducto> constructor)
    {
        constructor.HasKey(c => c.Id);

        constructor.Property(c => c.Nombre)
            .HasMaxLength(100)
            .IsRequired();

        constructor.Property(c => c.Descripcion)
            .HasMaxLength(250);

        constructor.Property(c => c.OrdenDisplay)
            .HasDefaultValue(0)
            .IsRequired();

        constructor.Property(c => c.Activo)
            .IsRequired();

        constructor.Property(c => c.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        constructor.HasIndex(c => c.Nombre)
            .IsUnique();
    }
}
