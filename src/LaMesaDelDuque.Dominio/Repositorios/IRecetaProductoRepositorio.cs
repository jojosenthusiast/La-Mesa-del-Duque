using LaMesaDelDuque.Dominio.Entidades;

namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IRecetaProductoRepositorio
{
    Task<RecetaProducto?> ObtenerPorProductoIdAsync(Guid productoId, CancellationToken cancelacion = default);
    Task AgregarAsync(RecetaProducto receta, CancellationToken cancelacion = default);
}
