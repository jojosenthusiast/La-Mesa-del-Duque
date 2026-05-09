using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages;

public sealed record ModuleLinkVm(string Label, string Page, string Description);

[Authorize]
public class IndexModel : PageModel
{
    public List<ModuleLinkVm> ModuleLinks { get; private set; } = [];

    public void OnGet()
    {
        var modulos = new List<ModuleLinkVm>();

        if (User.IsInRole("Administrador") || User.IsInRole("Encargado"))
            modulos.Add(new("Productos", "/Operaciones/Productos/Index", "Catálogo operativo y mantenimiento seguro."));

        if (User.IsInRole("Administrador") || User.IsInRole("Encargado") || User.IsInRole("Mesero"))
            modulos.Add(new("Mesas", "/Operaciones/Mesas/Index", "Estado del salón y acciones rápidas."));

        modulos.Add(new("Pedidos", "/Operaciones/Pedidos/Index", "Captura rápida de órdenes y totales visibles."));

        if (User.IsInRole("Administrador"))
            modulos.Add(new("Usuarios", "/Admin/Usuarios/Index", "Gestión de acceso y roles."));

        ModuleLinks = modulos;
    }
}
