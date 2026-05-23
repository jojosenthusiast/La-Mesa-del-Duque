using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class ZonaSalonConfiguracion : IEntityTypeConfiguration<ZonaSalon>
{
    public void Configure(EntityTypeBuilder<ZonaSalon> constructor)
    {
        constructor.HasKey(z => z.Id);

        constructor.Property(z => z.Nombre)
            .HasMaxLength(100)
            .IsRequired();

        constructor.Property(z => z.Orden)
            .HasDefaultValue(0)
            .IsRequired();

        constructor.Property(z => z.Activa)
            .IsRequired();

        constructor.Property(z => z.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        constructor.HasIndex(z => z.Nombre)
            .IsUnique();
    }
}
