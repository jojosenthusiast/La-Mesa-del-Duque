using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class RestauranteConfigConfiguracion : IEntityTypeConfiguration<RestauranteConfig>
{
    public void Configure(EntityTypeBuilder<RestauranteConfig> constructor)
    {
        constructor.HasKey(r => r.Id);

        constructor.Property(r => r.Id)
            .ValueGeneratedNever();

        constructor.Property(r => r.Nombre)
            .HasMaxLength(150)
            .IsRequired();

        constructor.Property(r => r.Direccion)
            .HasMaxLength(300)
            .IsRequired();

        constructor.Property(r => r.Telefono)
            .HasMaxLength(20);

        constructor.Property(r => r.HorarioApertura)
            .HasColumnType("time without time zone")
            .IsRequired();

        constructor.Property(r => r.HorarioCierre)
            .HasColumnType("time without time zone")
            .IsRequired();

        constructor.Property(r => r.CantidadMesas)
            .IsRequired();

        constructor.Property(r => r.DatosTicketJson)
            .HasColumnType("text");

        constructor.Property(r => r.PeriodoGraciaMinutos)
            .HasDefaultValue(5)
            .IsRequired();

        constructor.Property(r => r.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        constructor.Property(r => r.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        constructor.ToTable(t =>
            t.HasCheckConstraint("CK_RestauranteConfig_CantidadMesas", "\"CantidadMesas\" > 0"));
    }
}
