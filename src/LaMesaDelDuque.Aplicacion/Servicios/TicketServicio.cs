using System.Text;
using System.Text.Encodings.Web;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Repositorios;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface ITicketServicio
{
    Task<string> GenerarHtmlTicketAsync(Guid pedidoId, CancellationToken cancelacion = default);
}

public class TicketServicio : ITicketServicio
{
    private readonly IUnidadDeTrabajo _uot;

    public TicketServicio(IUnidadDeTrabajo uot)
    {
        _uot = uot;
    }

    public async Task<string> GenerarHtmlTicketAsync(Guid pedidoId, CancellationToken cancelacion = default)
    {
        var pedido = await _uot.Pedidos.ObtenerConDetallesAsync(pedidoId, cancelacion)
            ?? throw new ArgumentException("Pedido no encontrado.");

        // Obtener pago si existe (para mostrar método y referencia)
        var pagos = await _uot.Pagos.ObtenerDelDiaAsync(DateOnly.FromDateTime(DateTime.UtcNow), cancelacion);
        var cuentas = await _uot.Cuentas.ObtenerPorPedidoAsync(pedidoId, cancelacion);
        var cuentaIds = cuentas.Select(c => c.Id).ToHashSet();
        var pago = pagos.FirstOrDefault(p => cuentaIds.Contains(p.CuentaId));

        var ahora = DateTime.Now;
        var ticketId = pedidoId.ToString("N")[..8].ToUpper();
        var servicio = pedido.TipoServicio switch
        {
            TipoServicio.ComerAqui => "Comer aquí",
            TipoServicio.Domicilio => "Domicilio",
            _ => "Para llevar"
        };
        var metodoPagoLabel = pago?.Metodo switch
        {
            MetodoPago.Tarjeta => "Tarjeta",
            MetodoPago.Transferencia => "QR / Transf.",
            _ => "Efectivo"
        };

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
        sb.AppendLine(".pago{font-size:11px;text-align:right;color:#555;margin-top:4px}");
        sb.AppendLine(".ftr{text-align:center;font-size:9px;color:#6B6F76;margin-top:12px;border-top:1px dashed #ccc;padding-top:6px}");
        sb.AppendLine("@media print{body{width:80mm}@page{size:80mm auto;margin:0}}");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine("<div class=\"hdr\"><h1>La Mesa del Duque</h1><p>Ticket de compra</p></div>");
        sb.AppendLine("<div class=\"info\">");
        sb.AppendLine("<span><b>Ticket:</b> " + E(ticketId) + "</span>");
        sb.AppendLine("<span><b>Fecha:</b> " + E(ahora.ToString("dd/MM/yyyy HH:mm")) + "</span>");
        sb.AppendLine("<span><b>Servicio:</b> " + E(servicio) + "</span>");
        if (pedido.TipoServicio == TipoServicio.ComerAqui && pedido.Mesa is not null)
            sb.AppendLine("<span><b>Mesa:</b> " + E(pedido.Mesa.Numero.ToString()) + "</span>");
        if (pedido.TipoServicio == TipoServicio.Domicilio)
        {
            sb.AppendLine("<span><b>Cliente:</b> " + E(pedido.NombreClienteEntrega) + "</span>");
            sb.AppendLine("<span><b>Teléfono:</b> " + E(pedido.TelefonoEntrega) + "</span>");
            sb.AppendLine("<span><b>Dirección:</b> " + E(pedido.DireccionEntrega) + "</span>");
            if (!string.IsNullOrWhiteSpace(pedido.ReferenciaEntrega))
                sb.AppendLine("<span><b>Referencia:</b> " + E(pedido.ReferenciaEntrega) + "</span>");
        }
        sb.AppendLine("</div>");
        sb.AppendLine("<table>");

        foreach (var d in pedido.Detalles)
        {
            sb.AppendLine("<tr><td>" + d.Cantidad + "x " + E(d.Producto.Nombre) + "</td><td style=\"text-align:right\">$" + d.Subtotal.ToString("F2") + "</td></tr>");
        }

        sb.AppendLine("</table>");
        sb.AppendLine("<div class=\"total\">Total: $" + pedido.Total.ToString("F2") + "</div>");

        if (pago != null)
        {
            sb.AppendLine("<div class=\"pago\">Pago: " + E(metodoPagoLabel) + "</div>");
            if (!string.IsNullOrWhiteSpace(pago.ReferenciaPos))
                sb.AppendLine("<div class=\"pago\">Auth: " + E(pago.ReferenciaPos) + "</div>");
        }

        sb.AppendLine("<div class=\"ftr\"><p>Gracias por su visita</p><p>Este ticket no es factura fiscal</p></div>");
        sb.AppendLine("</body></html>");

        return sb.ToString();
    }

    private static string E(string? valor) => HtmlEncoder.Default.Encode(valor ?? string.Empty);
}
