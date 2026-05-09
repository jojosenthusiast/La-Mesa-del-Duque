namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IUnidadDeTrabajo
{
    ICategoriaProductoRepositorio Categorias { get; }
    IProductoRepositorio Productos { get; }
    IIngredienteRepositorio Ingredientes { get; }
    IMesaRepositorio Mesas { get; }
    IPedidoRepositorio Pedidos { get; }
    IRecetaProductoRepositorio RecetasProductos { get; }
    IUsuarioRepositorio Usuarios { get; }
    IAuditoriaRepositorio Auditorias { get; }

    Task<int> GuardarCambiosAsync(CancellationToken cancelacion = default);
}
