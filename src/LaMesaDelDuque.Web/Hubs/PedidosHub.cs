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

    public async Task UnirseAGrupo(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task SalirDeGrupo(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task MarcarAgotado(Guid productoId)
    {
        await Clients.All.SendAsync("ProductoAgotado", productoId);
    }

    public async Task ReactivarProducto(Guid productoId)
    {
        await Clients.All.SendAsync("ProductoReactivado", productoId);
    }
}
