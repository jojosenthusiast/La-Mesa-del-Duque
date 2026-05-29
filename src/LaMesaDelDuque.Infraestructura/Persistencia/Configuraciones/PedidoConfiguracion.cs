using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class PedidoConfiguracion : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> constructor)
    {
        constructor.HasKey(p => p.Id);

        constructor.Property(p => p.FechaCreacion)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        constructor.Property(p => p.TipoServicio)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        constructor.Property(p => p.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        constructor.Property(p => p.Estado)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        constructor.Property(p => p.MeseroAsignadoId)
            .IsRequired(false);

        // Total es calculado, no se persiste
        constructor.Ignore(p => p.Total);

        // FK sombra hacia Mesa
        constructor.Property<Guid?>("MesaId")
            .IsRequired(false);

        constructor.HasOne(p => p.Mesa)
            .WithMany()
            .HasForeignKey("MesaId")
            .OnDelete(DeleteBehavior.Restrict);

        constructor.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(p => p.MeseroAsignadoId)
            .OnDelete(DeleteBehavior.SetNull);

        constructor.HasIndex(p => p.MeseroAsignadoId);

        // Detalles mapeados desde la propiedad pública Detalles
        // EF Core descubre automáticamente el backing field _detalles
        constructor.HasMany(p => p.Detalles)
            .WithOne()
            .HasForeignKey("PedidoId")
            .OnDelete(DeleteBehavior.Cascade);

        // AutoInclude eliminado: causaba que EF marcara DetallePedido nuevo como Modified
        // (en lugar de Added) al hacer fixup de FKs durante DetectChanges, resultando en
        // DbUpdateConcurrencyException al intentar hacer UPDATE de una fila inexistente.
        // Los callers que necesitan Detalles deben usar Include() explícito.
    }
}
