using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages;

public sealed record ModuleLinkVm(string Label, string Page, string Description);

public class IndexModel : PageModel
{
    public List<ModuleLinkVm> ModuleLinks { get; private set; } = [];

    public void OnGet()
    {
        ModuleLinks =
        [
            new("Productos", "/Operaciones/Productos/Index", "Catálogo operativo y mantenimiento seguro."),
            new("Mesas", "/Operaciones/Mesas/Index", "Estado del salón y acciones rápidas."),
            new("Pedidos", "/Operaciones/Pedidos/Index", "Captura rápida de órdenes y totales visibles.")
        ];
    }
}
