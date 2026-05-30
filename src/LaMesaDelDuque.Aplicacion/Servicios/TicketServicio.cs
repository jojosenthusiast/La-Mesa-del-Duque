using System.Text;
using LaMesaDelDuque.Aplicacion.Dtos;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface ITicketServicio
{
    Task<string> GenerarHtmlTicketAsync(Guid pedidoId, CancellationToken cancelacion = default);
}

public class TicketServicio : ITicketServicio
{
    private readonly IPedidosServicio _pedidosServicio;

    public TicketServicio(IPedidosServicio pedidosServicio)
    {
        _pedidosServicio = pedidosServicio;
    }

    public async Task<string> GenerarHtmlTicketAsync(Guid pedidoId, CancellationToken cancelacion = default)
    {
        var pedidos = await _pedidosServicio.ListarPedidosActivosAsync();
        var pedido = pedidos.FirstOrDefault(p => p.Id == pedidoId)
            ?? throw new ArgumentException("Pedido no encontrado.");

        var ahora = DateTime.Now;
        var ticketId = pedidoId.ToString("N")[..8];
        var mesa = pedido.MesaNumero.HasValue ? pedido.MesaNumero.ToString() : "Para llevar";

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html lang=\"es\"><head><meta charset=\"utf-8\">");
        sb.AppendLine("<title>Ticket " + ticketId + "</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("*{margin:0;padding:0;box-sizing:border-box}");
        sb.AppendLine("body{font-family:'Courier New',monospace;font-size:13px;width:80mm;margin:0 auto;padding:10px 5px;color:#0F1B2D}");
        sb.AppendLine(".hdr{text-align:center;border-bottom:2px solid #C9A24E;padding-bottom:6px;margin-bottom:6px}");
        sb.AppendLine(".hdr h1{font-family:Georgia,serif;font-size:16px;color:#C9A24E}");
        sb.AppendLine(".hdr p{font-size:10px;color:#6B6F76}");
        sb.AppendLine(".info{font-size:11px;margin-bottom:6px}.info span{display:block}");
        sb.AppendLine("table{width:100%;border-collapse:collapse;margin:6px 0}");
        sb.AppendLine("td{padding:4px 8px;border-bottom:1px dashed #ccc}");
        sb.AppendLine(".total{text-align:right;font-size:16px;font-weight:bold;border-top:2px solid #C9A24E;padding-top:6px;margin-top:6px}");
        sb.AppendLine(".ftr{text-align:center;font-size:9px;color:#6B6F76;margin-top:12px;border-top:1px dashed #ccc;padding-top:6px}");
        sb.AppendLine("@media print{body{width:80mm}@page{size:80mm auto;margin:0}}");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine("<div class=\"hdr\"><h1>La Mesa del Duque</h1><p>Ticket de compra</p></div>");
        sb.AppendLine("<div class=\"info\"><span><b>Ticket:</b> " + ticketId + "</span>");
        sb.AppendLine("<span><b>Fecha:</b> " + ahora.ToString("dd/MM/yyyy HH:mm") + "</span>");
        sb.AppendLine("<span><b>Mesa:</b> " + mesa + "</span></div>");
        sb.AppendLine("<table>");

        foreach (var d in pedido.Detalles)
        {
            sb.AppendLine("<tr><td>" + d.Cantidad + "x " + d.ProductoNombre + "</td><td style=\"text-align:right\">$" + d.Subtotal.ToString("F2") + "</td></tr>");
        }

        sb.AppendLine("</table>");
        sb.AppendLine("<div class=\"total\">Total: $" + pedido.Total.ToString("F2") + "</div>");
        sb.AppendLine("<div class=\"ftr\"><p>Gracias por su visita</p><p>Este ticket no es factura fiscal</p></div>");
        sb.AppendLine("</body></html>");

        return sb.ToString();
    }
}
