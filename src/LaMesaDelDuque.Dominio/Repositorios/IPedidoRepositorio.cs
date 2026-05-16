using LaMesaDelDuque.Dominio.Entidades;

namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IPedidoRepositorio
{
    Task<Pedido?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default);
    Task<Pedido?> ObtenerConDetallesAsync(Guid id, CancellationToken cancelacion = default);
    Task<Pedido?> ObtenerConDetallesParaActualizarAsync(Guid id, CancellationToken cancelacion = default);
    Task<List<Pedido>> ObtenerTodosAsync(CancellationToken cancelacion = default);
    Task AgregarAsync(Pedido pedido, CancellationToken cancelacion = default);
    void Eliminar(Pedido pedido);
    Task<List<Pedido>> ObtenerPorMesaAsync(Guid mesaId, CancellationToken cancelacion = default);
    Task<Pedido?> ObtenerConCuentasParaActualizarAsync(Guid id, CancellationToken cancelacion = default);
}
