using LaMesaDelDuque.Aplicacion.Dtos;
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

    public async Task NotificarPedidoCreadoAsync(Guid pedidoId, EstadoPedido estado, CancellationToken cancelacion = default)
    {
        await EnviarAsync(new { tipo = "PedidoCreado", pedidoId, estado = estado.ToString() }, cancelacion);
        await EnviarEventoDirectoAsync("PedidoCreado", cancelacion, pedidoId, estado.ToString());
    }

    public async Task NotificarEstadoCambiadoAsync(Guid pedidoId, EstadoPedido nuevoEstado, CancellationToken cancelacion = default)
    {
        await EnviarAsync(new { tipo = "EstadoCambiado", pedidoId, estado = nuevoEstado.ToString() }, cancelacion);
        await EnviarEventoDirectoAsync("EstadoCambiado", cancelacion, pedidoId, nuevoEstado.ToString());
    }

    public async Task NotificarPedidoCanceladoAsync(Guid pedidoId, CancellationToken cancelacion = default)
    {
        await EnviarAsync(new { tipo = "PedidoCancelado", pedidoId }, cancelacion);
        await EnviarEventoDirectoAsync("PedidoCancelado", cancelacion, pedidoId);
    }

    public Task NotificarOrdenCocinaAsync(string estacion, OrdenCocinaDto dto, CancellationToken cancelacion = default) =>
        EnviarAGrupoAsync($"cocina-{estacion}", "NuevaOrden", dto, cancelacion);

    public Task NotificarItemListoAsync(string estacion, Guid ordenId, CancellationToken cancelacion = default) =>
        EnviarAGrupoAsync($"cocina-{estacion}", "ItemListo", ordenId, cancelacion);

    public Task NotificarItemRecuperadoAsync(string estacion, OrdenCocinaDto dto, CancellationToken cancelacion = default) =>
        EnviarAGrupoAsync($"cocina-{estacion}", "ItemRecuperado", dto, cancelacion);

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

    private async Task EnviarEventoDirectoAsync(string methodName, CancellationToken cancelacion, params object[] args)
    {
        try
        {
            switch (args.Length)
            {
                case 0:
                    await _hub.Clients.All.SendAsync(methodName, cancelacion);
                    break;
                case 1:
                    await _hub.Clients.All.SendAsync(methodName, args[0], cancelacion);
                    break;
                case 2:
                    await _hub.Clients.All.SendAsync(methodName, args[0], args[1], cancelacion);
                    break;
                default:
                    await _hub.Clients.All.SendAsync(methodName, args, cancelacion);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo emitir evento directo de pedido por SignalR: {MethodName}.", methodName);
        }
    }

    private async Task EnviarAGrupoAsync(string groupName, string methodName, object payload, CancellationToken cancelacion)
    {
        try
        {
            await _hub.Clients.Group(groupName).SendAsync(methodName, payload, cancelacion);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo emitir notificación de cocina por SignalR al grupo {GroupName}.", groupName);
        }
    }
}
