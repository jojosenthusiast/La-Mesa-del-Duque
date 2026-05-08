using LaMesaDelDuque.Aplicacion.Dtos;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface IPedidosServicio
{
    Task<PedidoDto> CrearPedidoAsync(Guid mesaId, List<DetalleCreacionDto> detalles, CancellationToken cancelacion = default);
    Task<PedidoDto> AgregarDetalleAsync(Guid pedidoId, Guid productoId, int cantidad, decimal precioUnitario, CancellationToken cancelacion = default);
    Task<PedidoDto> EliminarDetalleAsync(Guid pedidoId, Guid detalleId, CancellationToken cancelacion = default);
    Task<PedidoDto> ActualizarCantidadDetalleAsync(Guid pedidoId, Guid detalleId, int nuevaCantidad, CancellationToken cancelacion = default);
    Task MarcarEnPreparacionAsync(Guid pedidoId, CancellationToken cancelacion = default);
    Task PagarPedidoAsync(Guid pedidoId, CancellationToken cancelacion = default);
    Task CancelarPedidoAsync(Guid pedidoId, CancellationToken cancelacion = default);
    Task<PedidoDto?> ObtenerPedidoAsync(Guid pedidoId, CancellationToken cancelacion = default);
    Task<List<PedidoDto>> ListarPedidosActivosAsync(CancellationToken cancelacion = default);
}
