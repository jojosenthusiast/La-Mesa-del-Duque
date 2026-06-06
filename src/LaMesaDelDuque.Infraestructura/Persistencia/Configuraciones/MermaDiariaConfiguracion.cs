using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class MermaDiariaConfiguracion : IEntityTypeConfiguration<MermaDiaria>
{
    public void Configure(EntityTypeBuilder<MermaDiaria> constructor)
    {
        constructor.HasKey(m => m.Id);

        constructor.Property(m => m.CantidadDescartada)
            .HasPrecision(10, 3)
            .IsRequired();

        constructor.Property(m => m.CostoEstimado)
            .HasPrecision(10, 2)
            .IsRequired()
            .HasDefaultValue(0m);

        constructor.Property(m => m.Tipo)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        constructor.Property(m => m.Lote)
            .HasMaxLength(50);

        constructor.Property(m => m.Notas)
            .HasMaxLength(500);

        constructor.Property(m => m.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        constructor.HasOne(m => m.CierreDia)
            .WithMany()
            .HasForeignKey(m => m.CierreDiaId)
            .OnDelete(DeleteBehavior.Restrict);

        constructor.HasOne(m => m.Ingrediente)
            .WithMany()
            .HasForeignKey(m => m.IngredienteId)
            .OnDelete(DeleteBehavior.Restrict);

        constructor.HasOne(m => m.Usuario)
            .WithMany()
            .HasForeignKey(m => m.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        constructor.ToTable(t =>
        {
            t.HasCheckConstraint("CK_MermaDiaria_CantidadDescartada", "\"CantidadDescartada\" > 0");
            t.HasCheckConstraint("CK_MermaDiaria_CostoEstimado", "\"CostoEstimado\" >= 0");
        });

        constructor.HasIndex(m => m.CierreDiaId);
    }
}
