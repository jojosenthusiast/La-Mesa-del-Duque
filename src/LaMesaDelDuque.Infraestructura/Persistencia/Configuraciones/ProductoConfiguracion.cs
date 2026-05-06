using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class ProductoConfiguracion : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> constructor)
    {
        constructor.HasKey(p => p.Id);

        constructor.Property(p => p.Nombre)
            .HasMaxLength(300)
            .IsRequired();

        constructor.Property(p => p.Precio)
            .HasPrecision(10, 2)
            .IsRequired();

        constructor.Property(p => p.Activo)
            .IsRequired();

        constructor.Property(p => p.Descripcion)
            .HasMaxLength(1000);

        // FK sombra hacia CategoriaProducto
        constructor.Property<Guid>("CategoriaId")
            .IsRequired();

        constructor.HasOne(p => p.Categoria)
            .WithMany()
            .HasForeignKey("CategoriaId");
    }
}
