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
    }
}
