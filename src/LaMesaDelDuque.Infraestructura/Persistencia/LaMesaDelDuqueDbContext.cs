using LaMesaDelDuque.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Infraestructura.Persistencia;

public class LaMesaDelDuqueDbContext : DbContext
{
    public LaMesaDelDuqueDbContext(DbContextOptions<LaMesaDelDuqueDbContext> opciones)
        : base(opciones)
    {
    }

    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<RolPermiso> RolesPermisos => Set<RolPermiso>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<PedidoEstadoLog> PedidosEstadosLog => Set<PedidoEstadoLog>();
    public DbSet<ProductoPrecioHistorial> ProductosPreciosHistorial => Set<ProductoPrecioHistorial>();

    protected override void OnModelCreating(ModelBuilder constructorDeModelos)
    {
        constructorDeModelos.ApplyConfigurationsFromAssembly(typeof(LaMesaDelDuqueDbContext).Assembly);
    }
}
