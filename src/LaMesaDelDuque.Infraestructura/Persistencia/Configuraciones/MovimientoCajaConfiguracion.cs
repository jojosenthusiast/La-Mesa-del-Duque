using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class MovimientoCajaConfiguracion : IEntityTypeConfiguration<MovimientoCaja>
{
    public void Configure(EntityTypeBuilder<MovimientoCaja> builder)
    {
        builder.ToTable("MovimientosCaja");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Tipo).HasMaxLength(50).IsRequired();
        builder.Property(m => m.Motivo).HasMaxLength(300).IsRequired();
        builder.Property(m => m.Monto).HasPrecision(12, 2).IsRequired();

        builder.Property(m => m.FechaHora)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(m => m.UsuarioId).IsRequired();

        builder.HasIndex(m => m.TurnoCajaId)
            .HasDatabaseName("IX_MovimientosCaja_TurnoCajaId");
    }
}
