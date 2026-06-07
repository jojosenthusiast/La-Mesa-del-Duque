using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Web.Models.Operaciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages.Operaciones.Mesas;

[Authorize(Roles = "Administrador,Encargado,Mesero")]
public class IndexModel : PageModel
{
    private static readonly string[] EstadosOrdenados = ["Disponible", "Ocupada", "Reservada", "Mantenimiento", "Inactiva"];
    private readonly IMesasServicio _mesasServicio;
    private readonly IPedidosServicio _pedidosServicio;

    public IndexModel(IMesasServicio mesasServicio, IPedidosServicio pedidosServicio)
    {
        _mesasServicio = mesasServicio;
        _pedidosServicio = pedidosServicio;
    }

    [BindProperty]
    public MesasPageVm Vm { get; set; } = new();

    [TempData]
    public string? ToastSuccess { get; set; }

    [TempData]
    public string? ToastError { get; set; }

    public async Task OnGetAsync()
    {
        SetUiContext();
        await CargarDatosAsync();
    }

    public async Task<IActionResult> OnPostGuardarAsync()
    {
        SetUiContext();
        if (!(User.IsInRole("Administrador") || User.IsInRole("Encargado")))
        {
            return Forbid();
        }
        if (!ModelState.IsValid)
        {
            await CargarDatosAsync();
            return Page();
        }

        try
        {
            if (Vm.Form.Id.HasValue)
            {
                await _mesasServicio.ActualizarMesaAsync(Vm.Form.Id.Value, Vm.Form.Numero, Vm.Form.Capacidad);
                ToastSuccess = "Mesa actualizada correctamente.";
            }
            else
            {
                await _mesasServicio.CrearMesaAsync(Vm.Form.Numero, Vm.Form.Capacidad);
                ToastSuccess = "Mesa creada correctamente.";
            }

            return RedirectToPage();
        }
        catch (ReglaDominioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ToastError = ex.Message;
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ToastError = ex.Message;
        }

        await CargarDatosAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostCambiarEstadoAsync(Guid id, string nuevoEstado)
    {
        SetUiContext();
        var puedeGestionar = User.IsInRole("Administrador") || User.IsInRole("Encargado");
        var puedeLiberar = User.IsInRole("Mesero") && string.Equals(nuevoEstado, "Disponible", StringComparison.OrdinalIgnoreCase);
        if (!(puedeGestionar || puedeLiberar))
        {
            return Forbid();
        }
        try
        {
            // Al liberar una mesa con pedido activo, cancelarlo también: esto lo
            // quita del KDS de cocina (notificación PedidoCancelado) y devuelve stock.
            if (string.Equals(nuevoEstado, "Disponible", StringComparison.OrdinalIgnoreCase))
            {
                var activos = await _pedidosServicio.ListarPedidosActivosAsync();
                var delaMesa = activos.Where(p => p.MesaId == id).ToList();
                foreach (var pedido in delaMesa)
                {
                    try { await _pedidosServicio.CancelarPedidoAsync(pedido.Id); }
                    catch (ReglaDominioException) { /* pedido ya pagado/cancelado: se ignora */ }
                }
            }

            await _mesasServicio.CambiarEstadoMesaAsync(id, nuevoEstado);
            ToastSuccess = $"Mesa actualizada a estado {nuevoEstado}.";
        }
        catch (ReglaDominioException ex)
        {
            ToastError = ex.Message;
        }
        catch (ArgumentException ex)
        {
            ToastError = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDesactivarAsync(Guid id)
    {
        SetUiContext();
        if (!(User.IsInRole("Administrador") || User.IsInRole("Encargado")))
        {
            return Forbid();
        }
        try
        {
            await _mesasServicio.DesactivarMesaAsync(id);
            ToastSuccess = "Mesa desactivada correctamente.";
        }
        catch (ReglaDominioException ex)
        {
            ToastError = ex.Message;
        }
        catch (ArgumentException ex)
        {
            ToastError = ex.Message;
        }

        return RedirectToPage();
    }

    private async Task CargarDatosAsync()
    {
        Vm.Mesas = await _mesasServicio.ListarMesasAsync();

        var resumen = EstadosOrdenados.ToDictionary(e => e, _ => 0, StringComparer.OrdinalIgnoreCase);
        foreach (var mesa in Vm.Mesas)
        {
            if (!resumen.TryAdd(mesa.Estado, 1))
            {
                resumen[mesa.Estado]++;
            }
        }

        Vm.ResumenPorEstado = resumen;
    }

    private void SetUiContext()
    {
        if (ViewData is not null)
        {
            ViewData["ActiveTab"] = "Mesas";
        }
    }
}
