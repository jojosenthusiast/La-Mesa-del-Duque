using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LaMesaDelDuque.Web.Hubs;

[Authorize]
public class PedidosHub : Hub
{
}
