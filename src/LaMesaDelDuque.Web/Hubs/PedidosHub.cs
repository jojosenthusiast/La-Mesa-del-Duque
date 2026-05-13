using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LaMesaDelDuque.Web.Hubs;

[Authorize]
public class PedidosHub : Hub
{
    public async Task UnirseAPedido(Guid pedidoId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"pedido-{pedidoId}");
    }

    public async Task SalirDePedido(Guid pedidoId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"pedido-{pedidoId}");
    }
}
