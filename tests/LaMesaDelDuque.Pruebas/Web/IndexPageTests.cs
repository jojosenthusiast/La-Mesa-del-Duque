using LaMesaDelDuque.Web.Pages;

namespace LaMesaDelDuque.Pruebas.Web;

public class IndexPageTests
{
    [Fact]
    public void OnGet_sets_operational_summary_defaults()
    {
        var page = new IndexModel();

        page.OnGet();

        Assert.NotNull(page.ModuleLinks);
        Assert.Single(page.ModuleLinks); // solo Pedidos sin rol
        Assert.Equal("Pedidos", page.ModuleLinks[0].Label);
    }
}
