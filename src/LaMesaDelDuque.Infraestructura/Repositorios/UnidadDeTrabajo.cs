using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class UnidadDeTrabajo : IUnidadDeTrabajo
{
    private readonly LaMesaDelDuqueDbContext _contexto;

    public UnidadDeTrabajo(
        LaMesaDelDuqueDbContext contexto,
        CategoriaProductoRepositorio categoriaRepositorio,
        ProductoRepositorio productoRepositorio,
        IngredienteRepositorio ingredienteRepositorio,
        MesaRepositorio mesaRepositorio,
        PedidoRepositorio pedidoRepositorio,
        RolRepositorio rolRepositorio,
        UsuarioRepositorio usuarioRepositorio,
        AuditoriaRepositorio auditoriaRepositorio,
        RecetaProductoRepositorio recetaProductoRepositorio,
        OrdenCocinaRepositorio ordenCocinaRepositorio,
        CuentaRepositorio cuentaRepositorio,
        PagoRepositorio pagoRepositorio,
        ProveedorRepositorio? proveedorRepositorio = null,
        MermaRepositorio? mermaRepositorio = null,
        CierreDiaRepositorio? cierreDiaRepositorio = null,
        ZonaSalonRepositorio? zonaSalonRepositorio = null)
    {
        _contexto = contexto;
        Categorias = categoriaRepositorio;
        Productos = productoRepositorio;
        Ingredientes = ingredienteRepositorio;
        Mesas = mesaRepositorio;
        Pedidos = pedidoRepositorio;
        Roles = rolRepositorio;
        Usuarios = usuarioRepositorio;
        Auditorias = auditoriaRepositorio;
        RecetasProductos = recetaProductoRepositorio;
        OrdenesCocina = ordenCocinaRepositorio;
        Cuentas = cuentaRepositorio;
        Pagos = pagoRepositorio;
        Proveedores = proveedorRepositorio;
        Mermas = mermaRepositorio!;
        CierresDia = cierreDiaRepositorio!;
        ZonasSalon = zonaSalonRepositorio!;
    }

    // Overload compatible con tests previos al merge de mapa-visual (14 params + Zona opcional)
    public UnidadDeTrabajo(
        LaMesaDelDuqueDbContext contexto,
        CategoriaProductoRepositorio categoriaRepositorio,
        ProductoRepositorio productoRepositorio,
        IngredienteRepositorio ingredienteRepositorio,
        MesaRepositorio mesaRepositorio,
        PedidoRepositorio pedidoRepositorio,
        RolRepositorio rolRepositorio,
        UsuarioRepositorio usuarioRepositorio,
        AuditoriaRepositorio auditoriaRepositorio,
        RecetaProductoRepositorio recetaProductoRepositorio,
        OrdenCocinaRepositorio ordenCocinaRepositorio,
        CuentaRepositorio cuentaRepositorio,
        PagoRepositorio pagoRepositorio,
        ZonaSalonRepositorio zonaSalonRepositorio)
    {
        _contexto = contexto;
        Categorias = categoriaRepositorio;
        Productos = productoRepositorio;
        Ingredientes = ingredienteRepositorio;
        Mesas = mesaRepositorio;
        Pedidos = pedidoRepositorio;
        Roles = rolRepositorio;
        Usuarios = usuarioRepositorio;
        Auditorias = auditoriaRepositorio;
        RecetasProductos = recetaProductoRepositorio;
        OrdenesCocina = ordenCocinaRepositorio;
        Cuentas = cuentaRepositorio;
        Pagos = pagoRepositorio;
        ZonasSalon = zonaSalonRepositorio;
    }

    public ICategoriaProductoRepositorio Categorias { get; }
    public IProductoRepositorio Productos { get; }
    public IIngredienteRepositorio Ingredientes { get; }
    public IMesaRepositorio Mesas { get; }
    public IPedidoRepositorio Pedidos { get; }
    public IRecetaProductoRepositorio RecetasProductos { get; }
    public IRolRepositorio Roles { get; }
    public IUsuarioRepositorio Usuarios { get; }
    public IAuditoriaRepositorio Auditorias { get; }
    public IOrdenCocinaRepositorio OrdenesCocina { get; }
    public ICuentaRepositorio Cuentas { get; }
    public IPagoRepositorio Pagos { get; }
    public IProveedorRepositorio? Proveedores { get; }
    public IMermaRepositorio Mermas { get; } = null!;
    public ICierreDiaRepositorio CierresDia { get; } = null!;
    public IZonaSalonRepositorio ZonasSalon { get; } = null!;

    public async Task<int> GuardarCambiosAsync(CancellationToken cancelacion = default)
    {
        return await _contexto.SaveChangesAsync(cancelacion);
    }
}
