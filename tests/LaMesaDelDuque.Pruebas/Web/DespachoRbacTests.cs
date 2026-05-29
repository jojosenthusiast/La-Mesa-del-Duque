using System.Security.Claims;
using LaMesaDelDuque.Pruebas.Calidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DespachoIndexModel = LaMesaDelDuque.Web.Pages.Operaciones.Despacho.IndexModel;
using HomeIndexModel = LaMesaDelDuque.Web.Pages.IndexModel;

namespace LaMesaDelDuque.Pruebas.Web;

public class DespachoRbacTests
{
    [Fact]
    public void DespachoPage_DebePermitirSoloGestionYDespacho()
    {
        var attribute = typeof(DespachoIndexModel)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .OfType<AuthorizeAttribute>()
            .Single();

        var roles = attribute.Roles?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [];

        Assert.Contains("Administrador", roles);
        Assert.Contains("Encargado", roles);
        Assert.Contains("Despacho", roles);
        Assert.DoesNotContain("Cajero", roles);
        Assert.DoesNotContain("Mesero", roles);
    }

    [Theory]
    [InlineData("Administrador")]
    [InlineData("Encargado")]
    [InlineData("Despacho")]
    public void Home_DebeMostrarDespachoSoloAGestionYDespacho(string rol)
    {
        var page = CreateHomePageForRole(rol);

        page.OnGet();

        Assert.Contains(page.ModuleLinks, module => module.Label == "Despacho" && module.Page == "/Operaciones/Despacho/Index");
    }

    [Theory]
    [InlineData("Cajero")]
    [InlineData("Mesero")]
    [InlineData("Cocinero")]
    [InlineData("Gerente")]
    public void Home_NoDebeMostrarDespachoAOtrosRolesOperativos(string rol)
    {
        var page = CreateHomePageForRole(rol);

        page.OnGet();

        Assert.DoesNotContain(page.ModuleLinks, module => module.Label == "Despacho" || module.Page == "/Operaciones/Despacho/Index");
    }

    [Fact]
    public void Program_DebeSembrarYRepararRolDespachoDedicado()
    {
        var source = ReadSource("src", "LaMesaDelDuque.Web", "Program.cs");

        Assert.Contains("new Rol(\"Despacho\"", source);
        Assert.Contains("new Usuario(\"ana\"", source);
        Assert.Contains("[\"ana\"]", source);
        Assert.Contains("FirstOrDefaultAsync(r => r.Nombre == \"Despacho\")", source);
        Assert.DoesNotContain("Cobro en caja, despacho y cierre de turno", source);
    }

    [Fact]
    public void Login_DebeRedirigirDespachoASuBandeja()
    {
        var source = ReadSource("src", "LaMesaDelDuque.Web", "Pages", "Auth", "Login.cshtml.cs");

        Assert.Contains("\"Despacho\"", source);
        Assert.Contains("\"/Operaciones/Despacho/Index\"", source);
    }

    [Fact]
    public void Layouts_DebenExponerDespachoSoloEnNavegacionAutorizada()
    {
        var layout = ReadSource("src", "LaMesaDelDuque.Web", "Pages", "Shared", "_Layout.cshtml");
        var sidebar = ReadSource("src", "LaMesaDelDuque.Web", "Pages", "Shared", "_Sidebar.cshtml");

        Assert.Contains("esDespacho", layout);
        Assert.Contains("esDespacho ? \"Despacho\"", layout);
        Assert.Contains("asp-page=\"/Operaciones/Despacho/Index\"", sidebar);
        Assert.Contains("activeTab == \"Despacho\"", sidebar);
    }

    private static HomeIndexModel CreateHomePageForRole(string rol)
    {
        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, rol.ToLowerInvariant()),
                new Claim(ClaimTypes.Role, rol)
            ],
            authenticationType: "TestAuth");

        return new HomeIndexModel
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

    private static string ReadSource(params string[] paths) =>
        File.ReadAllText(Path.Combine([ProjectPaths.RepoRoot, .. paths]));
}
