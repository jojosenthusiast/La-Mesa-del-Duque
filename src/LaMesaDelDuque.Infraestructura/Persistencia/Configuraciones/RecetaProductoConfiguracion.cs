using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class RecetaProductoConfiguracion : IEntityTypeConfiguration<RecetaProducto>
{
    public void Configure(EntityTypeBuilder<RecetaProducto> constructor)
    {
        constructor.HasKey(x => x.Id);

        constructor.Property(x => x.Instrucciones)
            .HasColumnType("text")
            .IsRequired();

        constructor.HasIndex(x => x.ProductoId)
            .IsUnique();

        constructor.HasOne(x => x.Producto)
            .WithMany()
            .HasForeignKey(x => x.ProductoId)
            .OnDelete(DeleteBehavior.Cascade);

        constructor.HasMany(x => x.Ingredientes)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
