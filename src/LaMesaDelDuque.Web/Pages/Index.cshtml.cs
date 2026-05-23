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
        var autenticado = User?.Identity?.IsAuthenticated == true;
        var esAdmin = autenticado && User!.IsInRole("Administrador");
        var esEncargado = autenticado && User.IsInRole("Encargado");
        var esMesero = autenticado && User.IsInRole("Mesero");
        var esCocinero = autenticado && User.IsInRole("Cocinero");

        // Pedidos: Admin, Encargado, Mesero
        if (esAdmin || esEncargado || esMesero)
            modulos.Add(new("Pedidos", "/Operaciones/Pedidos/Index", "Captura rápida de órdenes y punto de venta."));

        // Cocina: Cocinero, Encargado, Admin
        if (esCocinero || esEncargado || esAdmin)
            modulos.Add(new("Cocina", "/Cocina/KDS", "Pantalla de cocina con órdenes pendientes."));

        // Mesas: Admin, Encargado, Mesero
        if (esAdmin || esEncargado || esMesero)
            modulos.Add(new("Mesas", "/Operaciones/Mesas/Index", "Gestión visual del salón y estados."));

        // Despacho: Admin, Encargado, Mesero
        if (esAdmin || esEncargado || esMesero)
            modulos.Add(new("Despacho", "/Operaciones/Despacho/Index", "Pedidos listos para entregar y liberar mesas."));

        // Productos: Admin, Encargado
        if (esAdmin || esEncargado)
            modulos.Add(new("Productos", "/Operaciones/Productos/Index", "Catálogo y recetas del menú."));

        // Inventario: Admin, Encargado
        if (esAdmin || esEncargado)
            modulos.Add(new("Inventario", "/Operaciones/Inventario/Index", "Ingredientes, proveedores y mermas."));

        // Cierre: Admin, Encargado
        if (esAdmin || esEncargado)
            modulos.Add(new("Cierre", "/Operaciones/Cierre/Index", "Apertura y cierre de caja diario."));

        // Usuarios: solo Admin
        if (esAdmin)
            modulos.Add(new("Usuarios", "/Admin/Usuarios/Index", "Gestión de acceso y roles del sistema."));

        ModuleLinks = modulos;
    }
}
