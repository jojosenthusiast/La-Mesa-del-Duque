using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class PromocionConfiguracion : IEntityTypeConfiguration<Promocion>
{
    public void Configure(EntityTypeBuilder<Promocion> constructor)
    {
        constructor.HasKey(p => p.Id);

        constructor.Property(p => p.Nombre)
            .HasMaxLength(150)
            .IsRequired();

        constructor.Property(p => p.Descripcion)
            .HasColumnType("text");

        constructor.Property(p => p.TipoDescuento)
            .HasMaxLength(20)
            .IsRequired();

        constructor.Property(p => p.ValorDescuento)
            .HasPrecision(10, 2)
            .IsRequired();

        constructor.Property(p => p.FechaInicio)
            .IsRequired();

        constructor.Property(p => p.FechaFin)
            .IsRequired();

        constructor.Property(p => p.Activo)
            .IsRequired()
            .HasDefaultValue(true);

        constructor.Property(p => p.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        constructor.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Promocion_TipoDescuento", "\"TipoDescuento\" IN ('porcentaje', 'fijo')");
            t.HasCheckConstraint("CK_Promocion_ValorDescuento", "\"ValorDescuento\" > 0");
        });
    }
}
