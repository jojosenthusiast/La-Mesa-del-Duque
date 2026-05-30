using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages.Operaciones.Despacho;

[Authorize(Roles = "Administrador,Encargado,Despacho")]
public class IndexModel : PageModel
{
    private readonly IPedidosServicio _pedidos;
    private readonly IDespachoServicio _despacho;
    private readonly ILogger<IndexModel> _logger;

    public List<PedidoDto> PedidosListos { get; set; } = [];

    [TempData] public string? ToastSuccess { get; set; }
    [TempData] public string? ToastError { get; set; }

    public IndexModel(IPedidosServicio pedidos, IDespachoServicio despacho, ILogger<IndexModel> logger)
    {
        _pedidos = pedidos;
        _despacho = despacho;
        _logger = logger;
    }

    public static int CalcularMinutosEsperaDespacho(DateTime referencia, DateTime ahoraUtc)
    {
        var referenciaUtc = NormalizarReferenciaUtc(referencia);
        var minutos = (int)Math.Floor((ahoraUtc - referenciaUtc).TotalMinutes);
        return Math.Max(0, minutos);
    }

    private static DateTime NormalizarReferenciaUtc(DateTime referencia)
    {
        return referencia.Kind switch
        {
            DateTimeKind.Utc => referencia,
            DateTimeKind.Local => referencia.ToUniversalTime(),
            _ => DateTime.SpecifyKind(referencia, DateTimeKind.Utc)
        };
    }

    public async Task OnGetAsync()
    {
        PedidosListos = await _pedidos.ListarListosParaDespachoAsync();
    }

    public async Task<IActionResult> OnPostDespacharAsync(Guid pedidoId)
    {
        try
        {
            await _despacho.DespacharPedidoAsync(pedidoId);
            ToastSuccess = "Pedido despachado. Mesa liberada.";
        }
        catch (ReglaDominioException ex)
        {
            ToastError = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al despachar pedido {PedidoId}", pedidoId);
            ToastError = "Ocurrió un error al despachar el pedido.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDespacharJsonAsync([FromBody] DespacharRequest req)
    {
        try
        {
            await _despacho.DespacharPedidoAsync(req.PedidoId);
            return new JsonResult(new { ok = true });
        }
        catch (ReglaDominioException ex)
        {
            return StatusCode(422, new { ok = false, error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en OnPostDespacharJsonAsync para pedido {PedidoId}", req.PedidoId);
            return StatusCode(500, new { ok = false, error = "Ocurrió un error interno." });
        }
    }

    public record DespacharRequest(Guid PedidoId);
}
