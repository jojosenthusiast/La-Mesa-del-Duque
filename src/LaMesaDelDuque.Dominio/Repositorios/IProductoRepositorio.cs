using LaMesaDelDuque.Dominio.Entidades;

namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IProductoRepositorio
{
    Task<Producto?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default);
    Task<Producto?> ObtenerConTrackingAsync(Guid id, CancellationToken cancelacion = default);
    Task<List<Producto>> ObtenerTodosAsync(CancellationToken cancelacion = default);
    Task<List<Producto>> ObtenerPorCategoriaAsync(Guid categoriaId, CancellationToken cancelacion = default);
    Task AgregarAsync(Producto producto, CancellationToken cancelacion = default);
    Task<bool> ExisteEnPedidosActivosAsync(Guid productoId, CancellationToken cancelacion = default);
}
