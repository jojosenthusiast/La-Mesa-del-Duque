using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class MotivoDescuentoConfiguracion : IEntityTypeConfiguration<MotivoDescuento>
{
    public void Configure(EntityTypeBuilder<MotivoDescuento> constructor)
    {
        constructor.ToTable("MotivosDescuento");

        constructor.HasKey(m => m.Id);

        constructor.Property(m => m.Nombre)
            .HasMaxLength(100)
            .IsRequired();

        constructor.Property(m => m.Descripcion)
            .HasMaxLength(300)
            .IsRequired(false);

        constructor.Property(m => m.Activo)
            .IsRequired();

        constructor.Property(m => m.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        constructor.HasIndex(m => m.Nombre)
            .HasDatabaseName("IX_MotivosDescuento_Nombre");
    }
}
