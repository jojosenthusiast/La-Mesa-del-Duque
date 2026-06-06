using LaMesaDelDuque.Dominio.Entidades;

namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IAlergenoRepositorio
{
    Task<List<Alergeno>> ObtenerTodosAsync(CancellationToken cancelacion = default);
    Task<List<Alergeno>> ObtenerActivosAsync(CancellationToken cancelacion = default);
    Task<Alergeno?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default);
    Task<List<ProductoAlergeno>> ObtenerPorProductoAsync(Guid productoId, CancellationToken cancelacion = default);
    Task AgregarProductoAlergenoAsync(ProductoAlergeno pa, CancellationToken cancelacion = default);
    Task EliminarProductoAlergenoAsync(Guid productoAlergenoId, CancellationToken cancelacion = default);
}
