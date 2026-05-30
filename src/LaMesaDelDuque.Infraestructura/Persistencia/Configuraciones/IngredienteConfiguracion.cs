using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class IngredienteConfiguracion : IEntityTypeConfiguration<Ingrediente>
{
    public void Configure(EntityTypeBuilder<Ingrediente> constructor)
    {
        constructor.HasKey(i => i.Id);

        constructor.ToTable(t =>
            t.HasCheckConstraint("CK_Ingrediente_StockActual_NoNegativo", "\"StockActual\" >= 0"));

        constructor.Property(i => i.Nombre)
            .HasMaxLength(150)
            .IsRequired();

        constructor.Property(i => i.UnidadMedida)
            .HasMaxLength(20)
            .IsRequired();

        constructor.Property(i => i.StockActual)
            .HasPrecision(10, 3)
            .IsConcurrencyToken()
            .IsRequired();

        constructor.Property(i => i.StockMinimo)
            .HasPrecision(10, 3)
            .IsRequired();

        constructor.Property(i => i.CostoUnitario)
            .HasPrecision(10, 2)
            .IsRequired();

        constructor.Property(i => i.Activo)
            .IsRequired();

        constructor.Property(i => i.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        constructor.Property(i => i.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        constructor.Property<Guid?>("ProveedorDefaultId");

        constructor.HasOne(i => i.ProveedorDefault)
            .WithMany()
            .HasForeignKey("ProveedorDefaultId")
            .OnDelete(DeleteBehavior.SetNull);

        constructor.HasIndex(i => i.Nombre)
            .IsUnique();
    }
}
