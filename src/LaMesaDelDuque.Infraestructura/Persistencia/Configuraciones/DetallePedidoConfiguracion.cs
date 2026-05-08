using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class DetallePedidoConfiguracion : IEntityTypeConfiguration<DetallePedido>
{
    public void Configure(EntityTypeBuilder<DetallePedido> constructor)
    {
        constructor.HasKey(d => d.Id);

        constructor.Property(d => d.Cantidad)
            .IsRequired();

        constructor.Property(d => d.PrecioUnitario)
            .HasPrecision(10, 2)
            .IsRequired();

        // Subtotal es calculado, no se persiste
        constructor.Ignore(d => d.Subtotal);

        // FK sombra hacia Producto
        constructor.Property<Guid>("ProductoId")
            .IsRequired();

        constructor.HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey("ProductoId");
    }
}
