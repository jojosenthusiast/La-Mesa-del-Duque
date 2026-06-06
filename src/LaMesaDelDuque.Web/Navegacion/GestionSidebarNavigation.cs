namespace LaMesaDelDuque.Web.Navegacion;

public sealed record GestionSidebarItem(
    string Label,
    string Page,
    string IconCss,
    string ActiveTab,
    string Title);

public sealed record GestionSidebarSection(
    string Label,
    IReadOnlyList<GestionSidebarItem> Items);

public static class GestionSidebarNavigation
{
    public static IReadOnlyList<GestionSidebarSection> Construir(bool esAdmin, bool esEncargado, bool esGerente)
    {
        var sections = new List<GestionSidebarSection>();

        var panel = new List<GestionSidebarItem>
        {
            Item("Dashboard", "/Admin/Dashboard/Dashboard", "bi bi-speedometer2", "Dashboard")
        };

        if (esAdmin || esGerente)
        {
            panel.Add(Item("Dashboard gerencial", "/Admin/Dashboard/Gerente", "bi bi-graph-up-arrow", "Dashboard Gerencial"));
        }

        sections.Add(new GestionSidebarSection("Panel", panel));

        if (esAdmin || esEncargado)
        {
            sections.Add(new GestionSidebarSection("Catálogo", new[]
            {
                Item("Productos", "/Operaciones/Productos/Index", "bi bi-box-seam", "Productos"),
                Item("Inventario", "/Operaciones/Inventario/Index", "bi bi-archive", "Inventario")
            }));

            sections.Add(new GestionSidebarSection("Salón", new[]
            {
                Item("Mesas", "/Operaciones/Mesas/Index", "bi bi-grid", "Mesas"),
                Item("Mapa del salón", "/Operaciones/Salon/Mapa", "bi bi-map", "Mapa")
            }));
        }

        if (esEncargado)
        {
            sections.Add(new GestionSidebarSection("Operación", new[]
            {
                Item("Pedidos", "/Operaciones/Pedidos/Index", "bi bi-receipt", "Pedidos"),
                Item("KDS cocina", "/Cocina/KDS", "bi bi-display", "Cocina"),
                Item("Despacho", "/Operaciones/Despacho/Index", "bi bi-truck", "Despacho"),
                Item("Caja", "/Operaciones/TurnoCaja/Index", "bi bi-cash-coin", "Caja"),
                Item("Transferir mesas", "/Operaciones/Mesero/Handoff", "bi bi-arrow-left-right", "MeseroHandoff")
            }));
        }

        if (esAdmin || esEncargado)
        {
            sections.Add(new GestionSidebarSection("Administración diaria", new[]
            {
                Item("Cierre de caja", "/Operaciones/Cierre/Index", "bi bi-cash-stack", "Cierre"),
                Item("Descuentos", "/Operaciones/Descuentos/Pendientes", "bi bi-percent", "Descuentos", "Descuentos pendientes")
            }));
        }

        var control = new List<GestionSidebarItem>
        {
            Item("Reportes", "/Admin/Reportes/Index", "bi bi-file-earmark-bar-graph", "Reportes")
        };

        if (esAdmin || esGerente)
        {
            control.Add(Item("Devoluciones", "/Admin/Devoluciones/Index", "bi bi-arrow-counterclockwise", "Devoluciones"));
            control.Add(Item("Auditoría", "/Admin/Auditoria/Index", "bi bi-shield-check", "Auditoría"));
        }

        sections.Add(new GestionSidebarSection("Control", control));

        if (esAdmin)
        {
            sections.Add(new GestionSidebarSection("Sistema", new[]
            {
                Item("Configuración", "/Admin/Configuracion/Index", "bi bi-sliders", "Configuración"),
                Item("Usuarios", "/Admin/Usuarios/Index", "bi bi-people", "Usuarios")
            }));
        }

        return sections;
    }

    private static GestionSidebarItem Item(string label, string page, string iconCss, string activeTab, string? title = null) =>
        new(label, page, iconCss, activeTab, title ?? label);
}
