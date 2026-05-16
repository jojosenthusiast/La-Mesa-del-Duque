using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class CuentaConfiguracion : IEntityTypeConfiguration<Cuenta>
{
    public void Configure(EntityTypeBuilder<Cuenta> constructor)
    {
        constructor.ToTable("Cuentas");

        constructor.HasKey(c => c.Id);

        constructor.Property(c => c.Numero)
            .IsRequired();

        constructor.Property(c => c.PropinaMonto)
            .HasPrecision(18, 2)
            .IsRequired();

        constructor.Property(c => c.MetodoPago)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired(false);

        constructor.Property(c => c.Estado)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        constructor.Property(c => c.FechaPago)
            .IsRequired(false);

        constructor.Property(c => c.Version)
            .HasColumnType("BLOB");

        constructor.HasOne<Pedido>()
            .WithMany(p => p.Cuentas)
            .HasForeignKey(c => c.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        constructor.HasIndex(c => c.PedidoId)
            .HasDatabaseName("IX_Cuentas_PedidoId");

        constructor.HasIndex(c => new { c.PedidoId, c.Estado })
            .HasDatabaseName("IX_Cuentas_PedidoId_Estado");

        constructor.HasMany(c => c.DetallesAsignados)
            .WithOne()
            .HasForeignKey(cd => cd.CuentaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
