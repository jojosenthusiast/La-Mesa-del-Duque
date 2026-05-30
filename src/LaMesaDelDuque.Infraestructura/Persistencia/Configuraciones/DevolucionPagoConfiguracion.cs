using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class DevolucionPagoConfiguracion : IEntityTypeConfiguration<DevolucionPago>
{
    public void Configure(EntityTypeBuilder<DevolucionPago> constructor)
    {
        constructor.ToTable("DevolucionesPago");

        constructor.HasKey(d => d.Id);

        constructor.Property(d => d.MontoDevuelto)
            .HasPrecision(18, 2)
            .IsRequired();

        constructor.Property(d => d.MetodoDevolucion)
            .HasMaxLength(30)
            .IsRequired();

        constructor.Property(d => d.MotivoDevolucion)
            .HasMaxLength(500)
            .IsRequired();

        constructor.Property(d => d.FechaHora)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        constructor.Property(d => d.StockReintegrado)
            .IsRequired();

        constructor.HasOne(d => d.PagoOriginal)
            .WithMany()
            .HasForeignKey(d => d.PagoOriginalId)
            .OnDelete(DeleteBehavior.Restrict);

        constructor.HasIndex(d => d.PagoOriginalId)
            .HasDatabaseName("IX_DevolucionesPago_PagoOriginalId");

        constructor.HasIndex(d => d.FechaHora)
            .HasDatabaseName("IX_DevolucionesPago_FechaHora");
    }
}
