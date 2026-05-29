using LaMesaDelDuque.Dominio.Entidades;

namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IPromocionRepositorio
{
    Task<List<PromocionProducto>> ObtenerActivasPorProductoAsync(Guid productoId, DateOnly fecha, CancellationToken ct = default);
}
