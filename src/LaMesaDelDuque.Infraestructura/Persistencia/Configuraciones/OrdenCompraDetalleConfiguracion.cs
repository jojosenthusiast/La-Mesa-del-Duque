using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class OrdenCompraDetalleConfiguracion : IEntityTypeConfiguration<OrdenCompraDetalle>
{
    public void Configure(EntityTypeBuilder<OrdenCompraDetalle> constructor)
    {
        constructor.HasKey(d => d.Id);

        constructor.Property(d => d.CantidadSolicitada)
            .HasPrecision(10, 3)
            .IsRequired();

        constructor.Property(d => d.CantidadRecibida)
            .HasPrecision(10, 3);

        constructor.Property(d => d.PrecioUnitario)
            .HasPrecision(10, 2)
            .IsRequired();

        constructor.HasOne(d => d.OrdenCompra)
            .WithMany()
            .HasForeignKey(d => d.OrdenCompraId)
            .OnDelete(DeleteBehavior.Cascade);

        constructor.HasOne(d => d.Ingrediente)
            .WithMany()
            .HasForeignKey(d => d.IngredienteId)
            .OnDelete(DeleteBehavior.Restrict);

        constructor.ToTable(t =>
        {
            t.HasCheckConstraint("CK_OrdenCompraDetalle_CantidadSolicitada", "\"CantidadSolicitada\" > 0");
            t.HasCheckConstraint("CK_OrdenCompraDetalle_CantidadRecibida", "\"CantidadRecibida\" IS NULL OR \"CantidadRecibida\" >= 0");
            t.HasCheckConstraint("CK_OrdenCompraDetalle_PrecioUnitario", "\"PrecioUnitario\" >= 0");
        });
    }
}
