using LaMesaDelDuque.Dominio.Entidades;

namespace LaMesaDelDuque.Dominio.Repositorios;

public interface ICategoriaProductoRepositorio
{
    Task<CategoriaProducto?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default);
    Task<CategoriaProducto?> ObtenerConTrackingAsync(Guid id, CancellationToken cancelacion = default);
    Task<List<CategoriaProducto>> ObtenerTodasAsync(CancellationToken cancelacion = default);
    Task AgregarAsync(CategoriaProducto categoria, CancellationToken cancelacion = default);
}
