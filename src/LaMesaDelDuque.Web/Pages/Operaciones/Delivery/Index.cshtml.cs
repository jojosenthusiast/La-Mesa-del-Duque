using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages.Operaciones.Delivery;

[Authorize(Roles = "Administrador,Encargado,Cajero")]
public class IndexModel : PageModel
{
    private readonly IDeliveryServicio _delivery;

    public IndexModel(IDeliveryServicio delivery) => _delivery = delivery;

    public List<DeliveryPedidoDto> Pedidos { get; set; } = [];
    public List<RepartidorDto> Repartidores { get; set; } = [];
    public DeliveryResumenDto Resumen { get; set; } = new();

    [TempData] public string? ToastSuccess { get; set; }
    [TempData] public string? ToastError { get; set; }

    public async Task OnGetAsync()
    {
        ViewData["ActiveTab"] = "Delivery";
        await CargarAsync();
    }

    private async Task CargarAsync()
    {
        Pedidos = await _delivery.ListarPedidosDomicilioAsync();
        Repartidores = await _delivery.ListarRepartidoresAsync();
        Resumen = await _delivery.ObtenerResumenAsync();
    }

    public async Task<IActionResult> OnPostAsignarAsync(Guid pedidoId, Guid repartidorId)
    {
        try
        {
            await _delivery.AsignarRepartidorAsync(pedidoId, repartidorId);
            ToastSuccess = "Repartidor asignado.";
        }
        catch (ReglaDominioException ex) { ToastError = ex.Message; }
        catch (Exception ex) { ToastError = ex.Message; }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEntregarAsync(Guid pedidoId)
    {
        try
        {
            await _delivery.MarcarEntregadoAsync(pedidoId);
            ToastSuccess = "Pedido marcado como entregado.";
        }
        catch (ReglaDominioException ex) { ToastError = ex.Message; }
        catch (Exception ex) { ToastError = ex.Message; }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDatosAsync(Guid pedidoId, string? direccion, string? telefono)
    {
        try
        {
            await _delivery.ActualizarDatosEntregaAsync(pedidoId, direccion, telefono);
            ToastSuccess = "Datos de entrega actualizados.";
        }
        catch (ReglaDominioException ex) { ToastError = ex.Message; }
        catch (Exception ex) { ToastError = ex.Message; }
        return RedirectToPage();
    }
}
