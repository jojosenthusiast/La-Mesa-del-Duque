using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Notificaciones;
using LaMesaDelDuque.Dominio.Enumeraciones;

namespace LaMesaDelDuque.Pruebas.Aplicacion;

internal sealed class NotificadorPedidosSpy : INotificadorPedidos
{
    public List<(Guid PedidoId, EstadoPedido Estado)> PedidosCreados { get; } = new();
    public List<(Guid PedidoId, EstadoPedido Estado)> EstadosCambiados { get; } = new();
    public List<Guid> PedidosCancelados { get; } = new();
    public List<(string Estacion, OrdenCocinaDto Dto)> OrdenesCocina { get; } = new();
    public List<(string Estacion, Guid OrdenId)> ItemsListos { get; } = new();
    public List<(string Estacion, OrdenCocinaDto Dto)> ItemsRecuperados { get; } = new();

    public Task NotificarPedidoCreadoAsync(Guid pedidoId, EstadoPedido estado, CancellationToken cancelacion = default)
    {
        PedidosCreados.Add((pedidoId, estado));
        return Task.CompletedTask;
    }

    public Task NotificarEstadoCambiadoAsync(Guid pedidoId, EstadoPedido nuevoEstado, CancellationToken cancelacion = default)
    {
        EstadosCambiados.Add((pedidoId, nuevoEstado));
        return Task.CompletedTask;
    }

    public Task NotificarPedidoCanceladoAsync(Guid pedidoId, CancellationToken cancelacion = default)
    {
        PedidosCancelados.Add(pedidoId);
        return Task.CompletedTask;
    }

    public Task NotificarOrdenCocinaAsync(string estacion, OrdenCocinaDto dto, CancellationToken cancelacion = default)
    {
        OrdenesCocina.Add((estacion, dto));
        return Task.CompletedTask;
    }

    public Task NotificarItemListoAsync(string estacion, Guid ordenId, CancellationToken cancelacion = default)
    {
        ItemsListos.Add((estacion, ordenId));
        return Task.CompletedTask;
    }

    public Task NotificarItemRecuperadoAsync(string estacion, OrdenCocinaDto dto, CancellationToken cancelacion = default)
    {
        ItemsRecuperados.Add((estacion, dto));
        return Task.CompletedTask;
    }
}
