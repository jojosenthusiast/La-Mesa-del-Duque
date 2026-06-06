using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace LaMesaDelDuque.Web.Pages.Operaciones.Descuentos;

[Authorize(Roles = "Administrador,Encargado,Cajero,Mesero")]
public class SolicitarModel : PageModel
{
    private readonly IDescuentoServicio _servicio;
    private readonly ILogger<SolicitarModel> _logger;

    [TempData] public string? ToastError { get; set; }
    [TempData] public string? ToastSuccess { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid PedidoId { get; set; }

    public List<MotivoDescuentoDto> Motivos { get; set; } = [];

    public SolicitarModel(IDescuentoServicio servicio, ILogger<SolicitarModel> logger)
    {
        _servicio = servicio;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (PedidoId == Guid.Empty)
            return RedirectToPage("/Operaciones/Pedidos/Index");

        Motivos = await _servicio.ListarMotivosAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        Guid pedidoId,
        Guid motivoId,
        string tipoDescuento,
        decimal valor,
        decimal montoAplicado)
    {
        var usuarioIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(usuarioIdStr, out var usuarioId))
        {
            ToastError = "No se pudo identificar el usuario autenticado.";
            return RedirectToPage(new { pedidoId });
        }

        // Para cortesía, valor es 100 (%)
        if (tipoDescuento == "cortesia")
            valor = 100m;

        try
        {
            await _servicio.SolicitarDescuentoAsync(
                pedidoId, motivoId, tipoDescuento, valor, montoAplicado, usuarioId);

            ToastSuccess = "Solicitud de descuento enviada. Pendiente de aprobación.";
            return RedirectToPage("/Operaciones/Pedidos/Index");
        }
        catch (ReglaDominioException ex)
        {
            ToastError = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al solicitar descuento para pedido {PedidoId}", pedidoId);
            ToastError = "Error interno al procesar la solicitud.";
        }

        PedidoId = pedidoId;
        Motivos = await _servicio.ListarMotivosAsync();
        return Page();
    }
}
