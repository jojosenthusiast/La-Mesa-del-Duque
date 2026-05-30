using LaMesaDelDuque.Aplicacion.Notificaciones;
using LaMesaDelDuque.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace LaMesaDelDuque.Web.Servicios;

public sealed class SignalRNotificadorProductos : INotificadorProductos
{
    private readonly IHubContext<PedidosHub> _hub;
    private readonly ILogger<SignalRNotificadorProductos> _logger;

    public SignalRNotificadorProductos(IHubContext<PedidosHub> hub, ILogger<SignalRNotificadorProductos> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public async Task NotificarProductoAgotadoAsync(Guid productoId, string nombreProducto, CancellationToken cancelacion = default)
    {
        try
        {
            await _hub.Clients.All.SendAsync("productoAgotado", productoId, nombreProducto, cancelacion);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo emitir notificación productoAgotado por SignalR.");
        }
    }

    public async Task NotificarProductoReactivadoAsync(Guid productoId, string nombreProducto, CancellationToken cancelacion = default)
    {
        try
        {
            await _hub.Clients.All.SendAsync("productoReactivado", productoId, nombreProducto, cancelacion);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo emitir notificación productoReactivado por SignalR.");
        }
    }
}
