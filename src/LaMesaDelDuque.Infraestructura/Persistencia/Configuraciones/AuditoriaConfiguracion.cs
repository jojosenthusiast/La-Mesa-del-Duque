using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class AuditoriaConfiguracion : IEntityTypeConfiguration<Auditoria>
{
    public void Configure(EntityTypeBuilder<Auditoria> constructor)
    {
        constructor.HasKey(a => a.Id);

        constructor.Property(a => a.Id)
            .UseIdentityAlwaysColumn();

        constructor.Property(a => a.TablaAfectada)
            .HasMaxLength(100)
            .IsRequired();

        constructor.Property(a => a.RegistroId)
            .IsRequired();

        constructor.Property(a => a.Accion)
            .HasMaxLength(10)
            .IsRequired();

        constructor.Property(a => a.DatosAnteriores)
            .HasColumnType("jsonb");

        constructor.Property(a => a.DatosNuevos)
            .HasColumnType("jsonb");

        constructor.Property(a => a.IpAddress)
            .HasMaxLength(45);

        constructor.Property(a => a.Fecha)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        constructor.HasOne(a => a.Usuario)
            .WithMany()
            .HasForeignKey(a => a.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        constructor.ToTable(t =>
            t.HasCheckConstraint("CK_Auditoria_Accion", "\"Accion\" IN ('INSERT', 'UPDATE', 'DELETE')"));

        constructor.HasIndex(a => new { a.TablaAfectada, a.RegistroId });
        constructor.HasIndex(a => a.Fecha);
        constructor.HasIndex(a => a.UsuarioId);
    }
}
