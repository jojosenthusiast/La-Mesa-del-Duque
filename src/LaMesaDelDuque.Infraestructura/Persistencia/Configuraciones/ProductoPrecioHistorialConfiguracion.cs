using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class ProductoPrecioHistorialConfiguracion : IEntityTypeConfiguration<ProductoPrecioHistorial>
{
    public void Configure(EntityTypeBuilder<ProductoPrecioHistorial> constructor)
    {
        constructor.HasKey(pph => pph.Id);
        constructor.Property(pph => pph.PrecioAnterior).HasPrecision(10, 2).IsRequired();
        constructor.Property(pph => pph.PrecioNuevo).HasPrecision(10, 2).IsRequired();
        constructor.Property(pph => pph.Razon).HasMaxLength(500).IsRequired();
        constructor.Property(pph => pph.FechaCambio).HasColumnType("timestamp with time zone").HasDefaultValueSql("NOW()").IsRequired();

        constructor.HasOne(pph => pph.Producto)
            .WithMany()
            .HasForeignKey(pph => pph.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        constructor.HasOne(pph => pph.Usuario)
            .WithMany()
            .HasForeignKey(pph => pph.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        constructor.HasIndex(pph => pph.ProductoId);
        constructor.HasIndex(pph => pph.FechaCambio);
    }
}
