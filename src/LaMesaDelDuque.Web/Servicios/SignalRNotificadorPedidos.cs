using LaMesaDelDuque.Aplicacion.Notificaciones;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace LaMesaDelDuque.Web.Servicios;

public sealed class SignalRNotificadorPedidos : INotificadorPedidos
{
    private readonly IHubContext<PedidosHub> _hub;
    private readonly ILogger<SignalRNotificadorPedidos> _logger;

    public SignalRNotificadorPedidos(IHubContext<PedidosHub> hub, ILogger<SignalRNotificadorPedidos> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public Task NotificarPedidoCreadoAsync(Guid pedidoId, EstadoPedido estado, CancellationToken cancelacion = default) =>
        EnviarAsync(new { tipo = "PedidoCreado", pedidoId, estado = estado.ToString() }, cancelacion);

    public Task NotificarEstadoCambiadoAsync(Guid pedidoId, EstadoPedido nuevoEstado, CancellationToken cancelacion = default) =>
        EnviarAsync(new { tipo = "EstadoCambiado", pedidoId, estado = nuevoEstado.ToString() }, cancelacion);

    public Task NotificarPedidoCanceladoAsync(Guid pedidoId, CancellationToken cancelacion = default) =>
        EnviarAsync(new { tipo = "PedidoCancelado", pedidoId }, cancelacion);

    private async Task EnviarAsync(object payload, CancellationToken cancelacion)
    {
        try
        {
            await _hub.Clients.All.SendAsync("RecibirNotificacionPedido", payload, cancelacion);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo emitir notificación de pedido por SignalR.");
        }
    }
}
