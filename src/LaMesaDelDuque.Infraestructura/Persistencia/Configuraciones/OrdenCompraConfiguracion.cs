using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class OrdenCompraConfiguracion : IEntityTypeConfiguration<OrdenCompra>
{
    public void Configure(EntityTypeBuilder<OrdenCompra> constructor)
    {
        constructor.HasKey(o => o.Id);

        constructor.Property(o => o.Estado)
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("solicitado");

        constructor.Property(o => o.FechaSolicitud)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        constructor.Property(o => o.FechaRecepcion)
            .HasColumnType("timestamp with time zone");

        constructor.Property(o => o.Notas)
            .HasColumnType("text");

        constructor.Property(o => o.ImpactoFallo)
            .HasColumnType("text");

        constructor.Property(o => o.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        constructor.Property(o => o.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        constructor.HasOne(o => o.Proveedor)
            .WithMany()
            .HasForeignKey(o => o.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        constructor.HasOne(o => o.Usuario)
            .WithMany()
            .HasForeignKey(o => o.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        constructor.ToTable(t =>
            t.HasCheckConstraint("CK_OrdenCompra_Estado", "\"Estado\" IN ('solicitado', 'en_camino', 'recibido', 'fallo')"));

        constructor.HasIndex(o => o.ProveedorId);
        constructor.HasIndex(o => o.FechaSolicitud);
    }
}
