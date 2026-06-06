using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class PedidoEstadoLogConfiguracion : IEntityTypeConfiguration<PedidoEstadoLog>
{
    public void Configure(EntityTypeBuilder<PedidoEstadoLog> constructor)
    {
        constructor.HasKey(pel => pel.Id);
        constructor.Property(pel => pel.EstadoAnterior).HasMaxLength(20).IsRequired();
        constructor.Property(pel => pel.EstadoNuevo).HasMaxLength(20).IsRequired();
        constructor.Property(pel => pel.Notas).HasMaxLength(500);
        constructor.Property(pel => pel.FechaCambio).HasColumnType("timestamp with time zone").HasDefaultValueSql("NOW()").IsRequired();

        constructor.HasOne<Pedido>()
            .WithMany()
            .HasForeignKey(pel => pel.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        constructor.HasOne(pel => pel.Usuario)
            .WithMany()
            .HasForeignKey(pel => pel.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        constructor.HasIndex(pel => pel.PedidoId);
        constructor.HasIndex(pel => pel.FechaCambio);
    }
}
