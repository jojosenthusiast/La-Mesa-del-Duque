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
    public DbSet<RecetaProducto> RecetasProductos => Set<RecetaProducto>();
    public DbSet<RecetaIngrediente> RecetasIngredientes => Set<RecetaIngrediente>();

    // Nuevas entidades Sprint 1
    public DbSet<RestauranteConfig> RestauranteConfigs => Set<RestauranteConfig>();
    public DbSet<Combo> Combos => Set<Combo>();
    public DbSet<ComboProducto> CombosProductos => Set<ComboProducto>();
    public DbSet<Promocion> Promociones => Set<Promocion>();
    public DbSet<PromocionProducto> PromocionesProductos => Set<PromocionProducto>();
    public DbSet<OrdenCompra> OrdenesCompra => Set<OrdenCompra>();
    public DbSet<OrdenCompraDetalle> OrdenesCompraDetalle => Set<OrdenCompraDetalle>();
    public DbSet<CierreDia> CierresDia => Set<CierreDia>();
    public DbSet<MermaDiaria> MermasDiarias => Set<MermaDiaria>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();

    // Nuevas entidades Sprint 2 — KDS
    public DbSet<OrdenCocina> OrdenesCocina => Set<OrdenCocina>();
    public DbSet<Pago> Pagos => Set<Pago>();

    // Nuevas entidades Sprint 3
    public DbSet<Alergeno> Alergenos => Set<Alergeno>();
    public DbSet<ProductoAlergeno> ProductosAlergenos => Set<ProductoAlergeno>();
    public DbSet<ZonaSalon> ZonasSalon => Set<ZonaSalon>();

    // Nuevas entidades §2.1 — Turno de caja
    public DbSet<TurnoCaja> TurnosCaja => Set<TurnoCaja>();
    public DbSet<MovimientoCaja> MovimientosCaja => Set<MovimientoCaja>();

    // Nuevas entidades §2.2 — Descuentos y cortesías
    public DbSet<MotivoDescuento> MotivosDescuento => Set<MotivoDescuento>();
    public DbSet<DescuentoAplicado> DescuentosAplicados => Set<DescuentoAplicado>();

    // Nuevas entidades §2.3 — Devoluciones de cobro
    public DbSet<DevolucionPago> DevolucionesPago => Set<DevolucionPago>();

    protected override void OnModelCreating(ModelBuilder constructorDeModelos)
    {
        constructorDeModelos.ApplyConfigurationsFromAssembly(typeof(LaMesaDelDuqueDbContext).Assembly);
    }
}
