using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class CategoriaProductoConfiguracion : IEntityTypeConfiguration<CategoriaProducto>
{
    public void Configure(EntityTypeBuilder<CategoriaProducto> constructor)
    {
        constructor.HasKey(c => c.Id);

        constructor.Property(c => c.Nombre)
            .HasMaxLength(200)
            .IsRequired();

        constructor.Property(c => c.Activo)
            .IsRequired();
    }
}
