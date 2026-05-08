using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class ComboProductoConfiguracion : IEntityTypeConfiguration<ComboProducto>
{
    public void Configure(EntityTypeBuilder<ComboProducto> constructor)
    {
        constructor.HasKey(cp => new { cp.ComboId, cp.ProductoId });

        constructor.Property(cp => cp.Cantidad)
            .IsRequired()
            .HasDefaultValue(1);

        constructor.HasOne(cp => cp.Combo)
            .WithMany()
            .HasForeignKey(cp => cp.ComboId)
            .OnDelete(DeleteBehavior.Cascade);

        constructor.HasOne(cp => cp.Producto)
            .WithMany()
            .HasForeignKey(cp => cp.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        constructor.ToTable(t =>
            t.HasCheckConstraint("CK_ComboProducto_Cantidad", "\"Cantidad\" > 0"));
    }
}
