using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages.Operaciones.Despacho;

[Authorize(Roles = "Administrador,Encargado,Mesero")]
public class IndexModel : PageModel
{
    private readonly IPedidosServicio _pedidos;
    private readonly IDespachoServicio _despacho;

    public List<PedidoDto> PedidosListos { get; set; } = [];

    [TempData] public string? ToastSuccess { get; set; }
    [TempData] public string? ToastError { get; set; }

    public IndexModel(IPedidosServicio pedidos, IDespachoServicio despacho)
    {
        _pedidos = pedidos;
        _despacho = despacho;
    }

    public async Task OnGetAsync()
    {
        var listos = await _pedidos.ListarListosParaDespachoAsync();
        PedidosListos = listos;
    }

    public async Task<IActionResult> OnPostDespacharAsync(Guid pedidoId)
    {
        try
        {
            await _despacho.DespacharPedidoAsync(pedidoId);
            ToastSuccess = "Pedido despachado. Mesa liberada.";
        }
        catch (Exception ex)
        {
            ToastError = ex.Message;
        }
        return RedirectToPage();
    }
}
