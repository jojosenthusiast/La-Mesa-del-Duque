using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class DescuentoAplicadoConfiguracion : IEntityTypeConfiguration<DescuentoAplicado>
{
    public void Configure(EntityTypeBuilder<DescuentoAplicado> constructor)
    {
        constructor.ToTable("DescuentosAplicados");

        constructor.HasKey(d => d.Id);

        constructor.Property(d => d.TipoDescuento)
            .HasMaxLength(20)
            .IsRequired();

        constructor.Property(d => d.Valor)
            .HasPrecision(18, 2)
            .IsRequired();

        constructor.Property(d => d.MontoAplicado)
            .HasPrecision(18, 2)
            .IsRequired();

        constructor.Property(d => d.Estado)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        constructor.Property(d => d.FechaSolicitud)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        constructor.Property(d => d.FechaResolucion)
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

        constructor.Property(d => d.NotaAutorizador)
            .HasMaxLength(500)
            .IsRequired(false);

        constructor.HasOne(d => d.Motivo)
            .WithMany()
            .HasForeignKey(d => d.MotivoId)
            .OnDelete(DeleteBehavior.Restrict);

        constructor.HasOne<Pedido>()
            .WithMany()
            .HasForeignKey(d => d.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        constructor.HasIndex(d => d.PedidoId)
            .HasDatabaseName("IX_DescuentosAplicados_PedidoId");

        constructor.HasIndex(d => d.Estado)
            .HasDatabaseName("IX_DescuentosAplicados_Estado");
    }
}
