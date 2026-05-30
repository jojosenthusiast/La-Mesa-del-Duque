using LaMesaDelDuque.Aplicacion.Notificaciones;
using LaMesaDelDuque.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace LaMesaDelDuque.Web.Servicios;

public sealed class SignalRNotificadorDashboard : INotificadorDashboard
{
    private readonly IHubContext<PedidosHub> _hub;
    private readonly ILogger<SignalRNotificadorDashboard> _logger;

    public SignalRNotificadorDashboard(IHubContext<PedidosHub> hub, ILogger<SignalRNotificadorDashboard> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public async Task NotificarMetricasInvalidadasAsync(CancellationToken cancelacion = default)
    {
        try
        {
            await _hub.Clients.Group("dashboard").SendAsync("MetricsInvalidated", cancelacion);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo emitir notificación de métricas invalidadas por SignalR.");
        }
    }
}
