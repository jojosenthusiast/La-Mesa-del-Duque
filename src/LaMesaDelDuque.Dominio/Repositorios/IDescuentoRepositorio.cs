using LaMesaDelDuque.Dominio.Entidades;

namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IDescuentoRepositorio
{
    Task<List<DescuentoAplicado>> ObtenerPorPedidoAsync(Guid pedidoId, CancellationToken cancelacion = default);
    Task<List<DescuentoAplicado>> ObtenerPendientesAsync(CancellationToken cancelacion = default);
    Task<DescuentoAplicado?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default);
    Task<List<DescuentoAplicado>> ObtenerPorRangoFechaAsync(DateTime desde, DateTime hasta, CancellationToken cancelacion = default);
    Task AgregarAsync(DescuentoAplicado descuento, CancellationToken cancelacion = default);
}
