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
        CuentaRepositorio cuentaRepositorio)
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
        Cuentas = cuentaRepositorio;
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
    public ICuentaRepositorio Cuentas { get; }

    public async Task<int> GuardarCambiosAsync(CancellationToken cancelacion = default)
    {
        return await _contexto.SaveChangesAsync(cancelacion);
    }
}
