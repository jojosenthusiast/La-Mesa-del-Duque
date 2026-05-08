using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class PromocionProductoConfiguracion : IEntityTypeConfiguration<PromocionProducto>
{
    public void Configure(EntityTypeBuilder<PromocionProducto> constructor)
    {
        constructor.HasKey(pp => new { pp.PromocionId, pp.ProductoId });

        constructor.HasOne(pp => pp.Promocion)
            .WithMany()
            .HasForeignKey(pp => pp.PromocionId)
            .OnDelete(DeleteBehavior.Cascade);

        constructor.HasOne(pp => pp.Producto)
            .WithMany()
            .HasForeignKey(pp => pp.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
