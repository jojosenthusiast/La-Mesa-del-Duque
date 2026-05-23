using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class CierreDiaConfiguracion : IEntityTypeConfiguration<CierreDia>
{
    public void Configure(EntityTypeBuilder<CierreDia> constructor)
    {
        constructor.HasKey(c => c.Id);

        constructor.Property(c => c.Fecha)
            .IsRequired();

        constructor.HasIndex(c => c.Fecha)
            .IsUnique();

        constructor.Property(c => c.TotalVentas)
            .HasPrecision(12, 2)
            .IsRequired();

        constructor.Property(c => c.TotalVentasEfectivo)
            .HasPrecision(12, 2)
            .IsRequired();

        constructor.Property(c => c.TotalVentasTarjeta)
            .HasPrecision(12, 2)
            .IsRequired();

        constructor.Property(c => c.TotalPedidos)
            .IsRequired();

        constructor.Property(c => c.TotalPedidosCancelados)
            .IsRequired();

        constructor.Property(c => c.TotalMermaValorizada)
            .HasPrecision(12, 2)
            .IsRequired();

        constructor.Property(c => c.ResumenJson)
            .HasColumnType("text");

        constructor.Property(c => c.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        constructor.Property(c => c.EsCerrado)
            .IsRequired()
            .HasDefaultValue(false);

        constructor.Property(c => c.CerradoEn)
            .HasColumnType("timestamp with time zone");

        constructor.HasOne(c => c.Usuario)
            .WithMany()
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
