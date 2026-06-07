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
        var esCajero = autenticado && User.IsInRole("Cajero");
        var esGerente = autenticado && User.IsInRole("Gerente");
        var esRepartidor = autenticado && User.IsInRole("Repartidor");

        // Pedidos: Admin, Encargado, Cajero
        if (esAdmin || esEncargado || esCajero)
            modulos.Add(new("Pedidos", "/Operaciones/Pedidos/Index", "Captura rápida de órdenes y punto de venta."));

        // Cocina: Cocinero, Encargado, Admin
        if (esCocinero || esEncargado || esAdmin)
            modulos.Add(new("Cocina", "/Cocina/KDS", "Pantalla de cocina con órdenes pendientes."));

        // Mesas: Admin, Encargado, Mesero
        if (esAdmin || esEncargado || esMesero)
            modulos.Add(new("Mesas", "/Operaciones/Mesas/Index", "Gestión visual del salón y estados."));

        // Mapa Salón: Admin, Encargado, Mesero
        if (esAdmin || esEncargado || esMesero)
            modulos.Add(new("Mapa Salón", "/Operaciones/Salon/Mapa", "Mapa visual interactivo con drag & drop."));

        // Despacho: Admin, Encargado, Cajero, Mesero
        if (esAdmin || esEncargado || esCajero || esMesero)
            modulos.Add(new("Despacho", "/Operaciones/Despacho/Index", "Pedidos listos para entregar y liberar mesas."));

        // Delivery: Admin, Encargado, Cajero
        if (esAdmin || esEncargado || esCajero)
            modulos.Add(new("Delivery", "/Operaciones/Delivery/Index", "Resumen de pedidos a domicilio y asignación de repartidores."));

        // Repartidor: vista liviana de envíos asignados
        if (esRepartidor)
            modulos.Add(new("Mis entregas", "/Operaciones/Repartidor/Index", "Direcciones, teléfonos y confirmación de entrega."));

        // Productos: Admin, Encargado
        if (esAdmin || esEncargado)
            modulos.Add(new("Productos", "/Operaciones/Productos/Index", "Catálogo y recetas del menú."));

        // Inventario: Admin, Encargado
        if (esAdmin || esEncargado)
            modulos.Add(new("Inventario", "/Operaciones/Inventario/Index", "Ingredientes, proveedores y mermas."));

        // Dashboard: Admin, Encargado, Gerente
        if (esAdmin || esEncargado || esGerente)
            modulos.Add(new("Dashboard", "/Admin/Dashboard/Dashboard", "KPIs y métricas operativas en tiempo real."));

        // Cierre: Admin, Encargado, Cajero (Gerente solo lectura via Dashboard)
        if (esAdmin || esEncargado || esCajero)
            modulos.Add(new("Cierre", "/Operaciones/Cierre/Index", "Apertura y cierre de caja diario."));

        // Caja: Admin, Encargado, Cajero
        if (esAdmin || esEncargado || esCajero)
            modulos.Add(new("Caja", "/Operaciones/TurnoCaja/Index", "Apertura, cierre de turno y Reporte Z."));

        // Dashboard Gerencial: Admin, Gerente
        if (esAdmin || esGerente)
            modulos.Add(new("Dashboard Gerencial", "/Admin/Dashboard/Gerente", "Análisis de período, tendencias y comparativas."));

        // Reportes: Admin, Encargado, Gerente
        if (esAdmin || esEncargado || esGerente)
            modulos.Add(new("Reportes", "/Admin/Reportes/Index", "Generar y descargar reportes en PDF y Excel."));

        // Descuentos: Admin, Encargado
        if (esAdmin || esEncargado)
            modulos.Add(new("Descuentos", "/Operaciones/Descuentos/Pendientes", "Aprobar descuentos y cortesías pendientes."));

        // Devoluciones: Admin, Gerente
        if (esAdmin || esGerente)
            modulos.Add(new("Devoluciones", "/Admin/Devoluciones/Index", "Gestionar devoluciones de cobro."));

        // Configuración: solo Admin
        if (esAdmin)
            modulos.Add(new("Configuración", "/Admin/Configuracion/Index", "Parámetros del restaurante."));

        // Auditoría: Admin, Gerente
        if (esAdmin || esGerente)
            modulos.Add(new("Auditoría", "/Admin/Auditoria/Index", "Registro de acciones y cambios del sistema."));

        // Usuarios: Admin
        if (esAdmin)
            modulos.Add(new("Usuarios", "/Admin/Usuarios/Index", "Gestión de acceso y roles del sistema."));


        ModuleLinks = modulos;
    }
}
