using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class RecetaIngredienteConfiguracion : IEntityTypeConfiguration<RecetaIngrediente>
{
    public void Configure(EntityTypeBuilder<RecetaIngrediente> constructor)
    {
        constructor.HasKey(x => x.Id);

        constructor.Property<Guid>("RecetaProductoId")
            .IsRequired();

        constructor.Property(x => x.CantidadRequerida)
            .HasPrecision(10, 3)
            .IsRequired();

        constructor.HasOne(x => x.Ingrediente)
            .WithMany()
            .HasForeignKey(x => x.IngredienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
