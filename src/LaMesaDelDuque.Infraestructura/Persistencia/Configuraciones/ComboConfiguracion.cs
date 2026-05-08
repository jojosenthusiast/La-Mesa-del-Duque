using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class ComboConfiguracion : IEntityTypeConfiguration<Combo>
{
    public void Configure(EntityTypeBuilder<Combo> constructor)
    {
        constructor.HasKey(c => c.Id);

        constructor.Property(c => c.Nombre)
            .HasMaxLength(150)
            .IsRequired();

        constructor.Property(c => c.Descripcion)
            .HasColumnType("text");

        constructor.Property(c => c.PrecioCombo)
            .HasPrecision(10, 2)
            .IsRequired();

        constructor.Property(c => c.Activo)
            .IsRequired()
            .HasDefaultValue(true);

        constructor.Property(c => c.FechaInicio)
            .IsRequired();

        constructor.Property(c => c.FechaFin);

        constructor.Property(c => c.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        constructor.ToTable(t =>
            t.HasCheckConstraint("CK_Combo_PrecioCombo", "\"PrecioCombo\" > 0"));
    }
}
