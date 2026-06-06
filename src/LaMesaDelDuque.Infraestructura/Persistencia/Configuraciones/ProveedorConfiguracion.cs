using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class ProveedorConfiguracion : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> constructor)
    {
        constructor.HasKey(p => p.Id);

        constructor.Property(p => p.Nombre)
            .HasMaxLength(200)
            .IsRequired();

        constructor.Property(p => p.Nit)
            .HasMaxLength(32)
            .IsRequired();

        constructor.Property(p => p.Contacto)
            .HasMaxLength(150);

        constructor.Property(p => p.Telefono)
            .HasMaxLength(20);

        constructor.Property(p => p.Email)
            .HasMaxLength(150);

        constructor.Property(p => p.Direccion)
            .HasMaxLength(300);

        constructor.Property(p => p.Activo)
            .IsRequired();

        constructor.Property(p => p.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        constructor.HasIndex(p => p.Nit)
            .IsUnique();
    }
}
