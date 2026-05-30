using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class TurnoCajaConfiguracion : IEntityTypeConfiguration<TurnoCaja>
{
    public void Configure(EntityTypeBuilder<TurnoCaja> builder)
    {
        builder.ToTable("TurnosCaja");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.FondoInicial).HasPrecision(12, 2).IsRequired();
        builder.Property(t => t.EfectivoEsperado).HasPrecision(12, 2);
        builder.Property(t => t.EfectivoContado).HasPrecision(12, 2);
        builder.Property(t => t.Diferencia).HasPrecision(12, 2);
        builder.Property(t => t.ObservacionCierre).HasMaxLength(500);
        builder.Property(t => t.FirmaDigital).HasMaxLength(100);

        builder.Property(t => t.FechaApertura)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(t => t.FechaCierre)
            .HasColumnType("timestamp with time zone");

        builder.HasOne(t => t.Cajero)
            .WithMany()
            .HasForeignKey(t => t.CajeroId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Movimientos)
            .WithOne()
            .HasForeignKey(m => m.TurnoCajaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.CajeroId)
            .HasDatabaseName("IX_TurnosCaja_CajeroId");

        builder.HasIndex(t => t.Cerrado)
            .HasDatabaseName("IX_TurnosCaja_Cerrado");
    }
}
