using LaMesaDelDuque.Aplicacion.Notificaciones;
using LaMesaDelDuque.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace LaMesaDelDuque.Web.Servicios;

public sealed class SignalRNotificadorSalon : INotificadorSalon
{
    private readonly IHubContext<PedidosHub> _hub;
    private readonly ILogger<SignalRNotificadorSalon> _logger;

    public SignalRNotificadorSalon(IHubContext<PedidosHub> hub, ILogger<SignalRNotificadorSalon> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public Task NotificarMesaMovidaAsync(Guid mesaId, int posX, int posY, CancellationToken cancelacion = default) =>
        EnviarAsync("MesaMovida", new { mesaId, posX, posY }, cancelacion);

    public Task NotificarMesaActualizadaAsync(Guid mesaId, string estado, CancellationToken cancelacion = default) =>
        EnviarAsync("MesaActualizada", new { mesaId, estado }, cancelacion);

    private async Task EnviarAsync(string methodName, object payload, CancellationToken cancelacion)
    {
        try
        {
            await _hub.Clients.Group("salon").SendAsync(methodName, payload, cancelacion);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo emitir notificación de salón por SignalR.");
        }
    }
}
