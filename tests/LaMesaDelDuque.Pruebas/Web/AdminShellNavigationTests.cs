using System.Security.Claims;
using LaMesaDelDuque.Pruebas.Calidad;
using LaMesaDelDuque.Web.Pages;
using LaMesaDelDuque.Web.Navegacion;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Pruebas.Web;

public class AdminShellNavigationTests
{

    [Fact]
    public void GestionSidebarNavigation_Administrador_NoIncluyeOperacionDeTurno()
    {
        var items = Flatten(GestionSidebarNavigation.Construir(esAdmin: true, esEncargado: false, esGerente: false));

        Assert.DoesNotContain(items, item => item.Page is "/Cocina/KDS" or "/Operaciones/Despacho/Index" or "/Operaciones/Pedidos/Index" or "/Operaciones/TurnoCaja/Index" or "/Operaciones/Mesero/Handoff");
        Assert.Contains(items, item => item.Page == "/Admin/Auditoria/Index" && item.Label == "Auditoría");
        Assert.Contains(items, item => item.Page == "/Admin/Configuracion/Index" && item.Label == "Configuración");
    }

    [Fact]
    public void GestionSidebarNavigation_Encargado_ConservaOperacionDeTurno()
    {
        var items = Flatten(GestionSidebarNavigation.Construir(esAdmin: false, esEncargado: true, esGerente: false));

        Assert.Contains(items, item => item.Page == "/Cocina/KDS" && item.Label == "KDS cocina");
        Assert.Contains(items, item => item.Page == "/Operaciones/Despacho/Index" && item.Label == "Despacho");
        Assert.Contains(items, item => item.Page == "/Operaciones/Mesero/Handoff" && item.Label == "Transferir mesas");
        Assert.DoesNotContain(items, item => item.Page == "/Admin/Usuarios/Index");
    }

    [Fact]
    public void GestionSidebarNavigation_Gerente_DebeVerSoloControlGerencial()
    {
        var items = Flatten(GestionSidebarNavigation.Construir(esAdmin: false, esEncargado: false, esGerente: true));

        Assert.Contains(items, item => item.Page == "/Admin/Dashboard/Gerente");
        Assert.Contains(items, item => item.Page == "/Admin/Reportes/Index");
        Assert.Contains(items, item => item.Page == "/Admin/Auditoria/Index");
        Assert.DoesNotContain(items, item => item.Page.StartsWith("/Operaciones/", StringComparison.Ordinal));
    }

    [Fact]
    public void Sidebar_DebeRenderizarDesdeContratoDeNavegacionPorRol()
    {
        var sidebar = ReadWebFile("Pages", "Shared", "_Sidebar.cshtml");

        Assert.Contains("GestionSidebarNavigation.Construir", sidebar);
        Assert.Contains("foreach (var section in sidebarSections)", sidebar);
        Assert.Contains("foreach (var item in section.Items)", sidebar);
    }

    [Fact]
    public void Home_Administrador_NoDebeMostrarModulosOperativosDeTurno()
    {
        var page = CreateHomePageForRole("Administrador");

        page.OnGet();

        Assert.DoesNotContain(page.ModuleLinks, module => module.Label is "Pedidos" or "Cocina" or "Despacho" or "Caja" or "Transferir mesas");
        Assert.DoesNotContain(page.ModuleLinks, module => module.Page is "/Cocina/KDS" or "/Operaciones/Despacho/Index" or "/Operaciones/Pedidos/Index" or "/Operaciones/TurnoCaja/Index" or "/Operaciones/Mesero/Handoff");
    }

    [Fact]
    public void Home_Administrador_DebePriorizarGestionControlYDefensa()
    {
        var page = CreateHomePageForRole("Administrador");

        page.OnGet();

        Assert.Contains(page.ModuleLinks, module => module.Label == "Dashboard" && module.Page == "/Admin/Dashboard/Dashboard");
        Assert.Contains(page.ModuleLinks, module => module.Label == "Reportes" && module.Page == "/Admin/Reportes/Index");
        Assert.Contains(page.ModuleLinks, module => module.Label == "Auditoría" && module.Page == "/Admin/Auditoria/Index");
        Assert.Contains(page.ModuleLinks, module => module.Label == "Configuración" && module.Page == "/Admin/Configuracion/Index");
        Assert.Contains(page.ModuleLinks, module => module.Label == "Usuarios" && module.Page == "/Admin/Usuarios/Index");
    }

    [Fact]
    public void Layout_GerenteDebeUsarShellDeGestionConRolVisible()
    {
        var layout = ReadWebFile("Pages", "Shared", "_Layout.cshtml");

        Assert.Contains("var esGerente", layout);
        Assert.Contains("esAdmin || esEncargado || esGerente", layout);
        Assert.Contains("esGerente ?", layout);
        Assert.Contains("Gerente", layout);
    }

    [Fact]
    public void Layout_OperativoDebeMostrarBotonVolver()
    {
        var layout = ReadWebFile("Pages", "Shared", "_Layout.cshtml");

        Assert.Contains("lmd-ops-strip__back", layout);
        Assert.Contains("Volver", layout);
    }

    [Fact]
    public void Sidebar_DebeExponerRutasAdministrativasYNoOperativasParaAdmin()
    {
        var adminItems = Flatten(GestionSidebarNavigation.Construir(esAdmin: true, esEncargado: false, esGerente: false));
        var sidebar = ReadWebFile("Pages", "Shared", "_Sidebar.cshtml");

        Assert.Contains(adminItems, item => item.Label == "Dashboard gerencial" && item.Page == "/Admin/Dashboard/Gerente");
        Assert.Contains(adminItems, item => item.Label == "Reportes" && item.Page == "/Admin/Reportes/Index");
        Assert.Contains(adminItems, item => item.Label == "Auditoría" && item.Page == "/Admin/Auditoria/Index");
        Assert.Contains(adminItems, item => item.Label == "Configuración" && item.Page == "/Admin/Configuracion/Index");
        Assert.DoesNotContain(adminItems, item => item.Page is "/Cocina/KDS" or "/Operaciones/Despacho/Index");
        Assert.Contains("GestionSidebarNavigation.Construir", sidebar);
    }

    [Fact]
    public void Sidebar_ActiveTabsDebenUsarValoresRealesDeLasPaginas()
    {
        var items = Flatten(GestionSidebarNavigation.Construir(esAdmin: true, esEncargado: true, esGerente: true));

        Assert.Contains(items, item => item.Page == "/Admin/Dashboard/Gerente" && item.ActiveTab == "Dashboard Gerencial");
        Assert.Contains(items, item => item.Page == "/Operaciones/TurnoCaja/Index" && item.ActiveTab == "Caja");
        Assert.Contains(items, item => item.Page == "/Operaciones/Mesero/Handoff" && item.ActiveTab == "MeseroHandoff");
        Assert.Contains(items, item => item.Page == "/Admin/Auditoria/Index" && item.ActiveTab == "Auditoría");
        Assert.Contains(items, item => item.Page == "/Admin/Configuracion/Index" && item.ActiveTab == "Configuración");
    }

    [Fact]
    public void Login_GerenteDebeEntrarADashboardGerencial()
    {
        var login = ReadWebFile("Pages", "Auth", "Login.cshtml.cs");

        Assert.Contains("\"Gerente\"", login);
        Assert.Contains("/Admin/Dashboard/Gerente", login);
    }

    [Fact]
    public void Sidebar_CollapseDebeSerReversibleYAccesible()
    {
        var sidebar = ReadWebFile("Pages", "Shared", "_Sidebar.cshtml");
        var css = ReadWebFile("wwwroot", "css", "sidebar.css");

        Assert.Contains("id=\"lmdSidebarBrandLink\"", sidebar);
        Assert.Contains("aria-expanded=\"true\"", sidebar);
        Assert.Contains("setCollapsed", sidebar);
        Assert.Contains("Expandir menú principal", sidebar);
        Assert.Contains("Cerrar sesión", sidebar);
        Assert.Contains("lmd-shell-with-sidebar--collapsed", css);
        Assert.Contains("calc(100% - var(--lmd-sidebar-ancho-col))", css);
        Assert.Contains(".lmd-sidebar--collapsed .lmd-sidebar__item span", css);
        Assert.Contains("max-width: 0", css);
    }

    private static IndexModel CreateHomePageForRole(string rol)
    {
        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, rol.ToLowerInvariant()),
                new Claim(ClaimTypes.Role, rol)
            ],
            authenticationType: "TestAuth");

        return new IndexModel
        {
            PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            }
        };
    }

    private static IReadOnlyList<GestionSidebarItem> Flatten(IReadOnlyList<GestionSidebarSection> sections) =>
        sections.SelectMany(section => section.Items).ToList();

    private static string ReadWebFile(params string[] segments) =>
        File.ReadAllText(Path.Combine([ProjectPaths.RepoRoot, "src", "LaMesaDelDuque.Web", .. segments]));
}
