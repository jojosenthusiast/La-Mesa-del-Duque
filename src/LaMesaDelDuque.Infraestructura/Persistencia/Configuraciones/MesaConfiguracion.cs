using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class MesaConfiguracion : IEntityTypeConfiguration<Mesa>
{
    public void Configure(EntityTypeBuilder<Mesa> constructor)
    {
        constructor.HasKey(m => m.Id);

        constructor.Property(m => m.Numero)
            .IsRequired();

        constructor.HasIndex(m => m.Numero)
            .IsUnique();

        constructor.Property(m => m.Capacidad)
            .IsRequired();

        constructor.Property(m => m.Estado)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        constructor.Property(m => m.Activa)
            .IsRequired();

        // Campos de posición para mapa visual (nullable para compatibilidad legacy)
        constructor.Property(m => m.PosicionX);
        constructor.Property(m => m.PosicionY);

        constructor.Property(m => m.ZonaId);
        constructor.HasOne(m => m.Zona)
            .WithMany()
            .HasForeignKey(m => m.ZonaId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        constructor.Property(m => m.Forma)
            .HasConversion<string>()
            .HasMaxLength(30);

        constructor.Property(m => m.Rotacion)
            .HasDefaultValue(0);

        constructor.Property(m => m.OcupadaDesde)
            .HasColumnType("timestamp with time zone");

        constructor.Property(m => m.GraciaHasta)
            .HasColumnType("timestamp with time zone");
    }
}
