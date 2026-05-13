using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Dominio.Enumeraciones;

namespace LaMesaDelDuque.Aplicacion.Notificaciones;

public interface INotificadorPedidos
{
    Task NotificarPedidoCreadoAsync(Guid pedidoId, EstadoPedido estado, CancellationToken cancelacion = default);
    Task NotificarEstadoCambiadoAsync(Guid pedidoId, EstadoPedido nuevoEstado, CancellationToken cancelacion = default);
    Task NotificarPedidoCanceladoAsync(Guid pedidoId, CancellationToken cancelacion = default);
    Task NotificarOrdenCocinaAsync(string estacion, OrdenCocinaDto dto, CancellationToken cancelacion = default);
    Task NotificarItemListoAsync(string estacion, Guid ordenId, CancellationToken cancelacion = default);
    Task NotificarItemRecuperadoAsync(string estacion, OrdenCocinaDto dto, CancellationToken cancelacion = default);
}
