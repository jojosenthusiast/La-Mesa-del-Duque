namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IUnidadDeTrabajo
{
    ICategoriaProductoRepositorio Categorias { get; }
    IProductoRepositorio Productos { get; }
    IMesaRepositorio Mesas { get; }
    IPedidoRepositorio Pedidos { get; }

    Task<int> GuardarCambiosAsync(CancellationToken cancelacion = default);
}
