namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IUnidadDeTrabajo
{
    ICategoriaProductoRepositorio Categorias { get; }
    IProductoRepositorio Productos { get; }
    IIngredienteRepositorio Ingredientes { get; }
    IMesaRepositorio Mesas { get; }
    IPedidoRepositorio Pedidos { get; }
    IRecetaProductoRepositorio RecetasProductos { get; }
    IRolRepositorio Roles { get; }
    IUsuarioRepositorio Usuarios { get; }
    IAuditoriaRepositorio Auditorias { get; }
    IOrdenCocinaRepositorio OrdenesCocina { get; }
    ICuentaRepositorio Cuentas { get; }
    IPagoRepositorio Pagos { get; }
    IProveedorRepositorio? Proveedores { get; }

    Task<int> GuardarCambiosAsync(CancellationToken cancelacion = default);
}
