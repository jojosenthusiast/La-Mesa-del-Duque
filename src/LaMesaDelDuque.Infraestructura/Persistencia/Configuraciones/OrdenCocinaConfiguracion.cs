using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class OrdenCocinaConfiguracion : IEntityTypeConfiguration<OrdenCocina>
{
    public void Configure(EntityTypeBuilder<OrdenCocina> constructor)
    {
        constructor.ToTable("OrdenesCocina");

        constructor.HasKey(o => o.Id);

        constructor.Property(o => o.ProductoNombre)
            .IsRequired()
            .HasMaxLength(150);

        constructor.Property(o => o.Notas)
            .HasMaxLength(250);

        constructor.Property(o => o.Alergenos)
            .HasMaxLength(150);

        constructor.Property(o => o.IngredientesQuitados)
            .HasMaxLength(150);

        constructor.Property(o => o.IngredientesExtra)
            .HasMaxLength(150);

        constructor.Property(o => o.TipoServicio)
            .HasMaxLength(50);

        constructor.Property(o => o.Curso)
            .HasConversion<string>()
            .HasMaxLength(20);

        constructor.Property(o => o.ProductoId)
            .IsRequired();

        constructor.Property(o => o.TiempoPreparacionMin)
            .IsRequired();

        constructor.HasIndex(o => new { o.Estado, o.HoraRecibido })
            .HasDatabaseName("IX_OrdenesCocina_Estado_HoraRecibido");

        constructor.HasIndex(o => new { o.Estacion, o.Estado })
            .HasDatabaseName("IX_OrdenesCocina_Estacion_Estado");
    }
}
