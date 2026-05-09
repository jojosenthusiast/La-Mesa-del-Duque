using LaMesaDelDuque.Dominio.Enumeraciones;

namespace LaMesaDelDuque.Aplicacion.Notificaciones;

public interface INotificadorPedidos
{
    Task NotificarPedidoCreadoAsync(Guid pedidoId, EstadoPedido estado, CancellationToken cancelacion = default);
    Task NotificarEstadoCambiadoAsync(Guid pedidoId, EstadoPedido nuevoEstado, CancellationToken cancelacion = default);
    Task NotificarPedidoCanceladoAsync(Guid pedidoId, CancellationToken cancelacion = default);
}
