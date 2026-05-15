using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class PagoConfiguracion : IEntityTypeConfiguration<Pago>
{
    public void Configure(EntityTypeBuilder<Pago> constructor)
    {
        constructor.ToTable("Pagos");

        constructor.HasKey(p => p.Id);

        constructor.Property(p => p.Monto)
            .HasPrecision(18, 2)
            .IsRequired();

        constructor.Property(p => p.PropinaMonto)
            .HasPrecision(18, 2)
            .IsRequired();

        constructor.Property(p => p.Metodo)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        constructor.Property(p => p.FechaPago)
            .IsRequired();

        constructor.HasOne<Cuenta>()
            .WithMany()
            .HasForeignKey(p => p.CuentaId)
            .OnDelete(DeleteBehavior.Cascade);

        constructor.HasIndex(p => p.CuentaId)
            .HasDatabaseName("IX_Pagos_CuentaId");
    }
}
