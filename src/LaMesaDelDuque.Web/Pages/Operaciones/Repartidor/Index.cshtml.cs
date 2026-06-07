using System.Security.Claims;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages.Operaciones.Repartidor;

[Authorize(Roles = "Repartidor,Administrador,Encargado")]
public class IndexModel : PageModel
{
    private readonly IDeliveryServicio _delivery;

    public IndexModel(IDeliveryServicio delivery) => _delivery = delivery;

    public List<DeliveryPedidoDto> Pedidos { get; set; } = [];
    public List<RepartidorDto> Repartidores { get; set; } = [];
    public bool EsSupervisor { get; set; }
    public Guid? RepartidorSeleccionadoId { get; set; }

    [TempData] public string? ToastSuccess { get; set; }
    [TempData] public string? ToastError { get; set; }

    public async Task OnGetAsync(Guid? repartidorId = null)
    {
        ViewData["ActiveTab"] = "Repartidor";
        EsSupervisor = User.IsInRole("Administrador") || User.IsInRole("Encargado");
        Repartidores = await _delivery.ListarRepartidoresAsync();

        var actual = EsSupervisor
            ? repartidorId ?? Repartidores.FirstOrDefault()?.Id
            : ObtenerUsuarioId();

        RepartidorSeleccionadoId = actual == Guid.Empty ? null : actual;
        Pedidos = RepartidorSeleccionadoId.HasValue
            ? await _delivery.ListarPedidosAsignadosAsync(RepartidorSeleccionadoId.Value)
            : [];
    }

    public async Task<IActionResult> OnPostEntregarAsync(Guid pedidoId, Guid? repartidorId = null)
    {
        try
        {
            await _delivery.MarcarEntregadoAsync(pedidoId);
            ToastSuccess = "Entrega confirmada.";
        }
        catch (ReglaDominioException ex) { ToastError = ex.Message; }
        catch (Exception ex) { ToastError = ex.Message; }

        return RedirectToPage(new { repartidorId });
    }

    private Guid ObtenerUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
