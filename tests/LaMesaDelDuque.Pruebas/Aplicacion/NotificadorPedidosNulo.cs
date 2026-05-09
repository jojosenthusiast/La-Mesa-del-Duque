using LaMesaDelDuque.Aplicacion.Notificaciones;
using LaMesaDelDuque.Dominio.Enumeraciones;

namespace LaMesaDelDuque.Pruebas.Aplicacion;

internal sealed class NotificadorPedidosNulo : INotificadorPedidos
{
    public Task NotificarPedidoCreadoAsync(Guid pedidoId, EstadoPedido estado, CancellationToken cancelacion = default) => Task.CompletedTask;

    public Task NotificarEstadoCambiadoAsync(Guid pedidoId, EstadoPedido nuevoEstado, CancellationToken cancelacion = default) => Task.CompletedTask;

    public Task NotificarPedidoCanceladoAsync(Guid pedidoId, CancellationToken cancelacion = default) => Task.CompletedTask;
}
