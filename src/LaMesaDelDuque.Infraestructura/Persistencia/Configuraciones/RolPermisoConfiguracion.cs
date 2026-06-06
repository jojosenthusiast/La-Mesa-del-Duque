using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LaMesaDelDuque.Infraestructura.Persistencia.Configuraciones;

internal class RolPermisoConfiguracion : IEntityTypeConfiguration<RolPermiso>
{
    public void Configure(EntityTypeBuilder<RolPermiso> constructor)
    {
        constructor.HasKey(rp => new { rp.RolId, rp.PermisoId });

        constructor.HasOne(rp => rp.Rol)
            .WithMany()
            .HasForeignKey(rp => rp.RolId)
            .OnDelete(DeleteBehavior.Cascade);

        constructor.HasOne(rp => rp.Permiso)
            .WithMany()
            .HasForeignKey(rp => rp.PermisoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
