using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class ProductoIngredienteConfiguracion : IEntityTypeConfiguration<ProductoIngrediente>
{
    public void Configure(EntityTypeBuilder<ProductoIngrediente> constructor)
    {
        constructor.HasKey(pi => new { pi.ProductoId, pi.IngredienteId });

        constructor.Property(pi => pi.CantidadRequerida)
            .HasPrecision(10, 3)
            .IsRequired();

        constructor.HasOne(pi => pi.Producto)
            .WithMany(p => p.Ingredientes)
            .HasForeignKey(pi => pi.ProductoId)
            .OnDelete(DeleteBehavior.Cascade);

        constructor.HasOne(pi => pi.Ingrediente)
            .WithMany()
            .HasForeignKey(pi => pi.IngredienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
