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
        Assert.Equal(3, page.ModuleLinks.Count);
    }

    [Fact]
    public void ModuleLinks_contain_all_three_modules()
    {
        var page = new IndexModel();

        page.OnGet();

        var labels = page.ModuleLinks.Select(m => m.Label).ToList();
        Assert.Contains("Productos", labels);
        Assert.Contains("Mesas", labels);
        Assert.Contains("Pedidos", labels);
    }

    [Fact]
    public void ModuleLinks_have_valid_page_references()
    {
        var page = new IndexModel();

        page.OnGet();

        Assert.All(page.ModuleLinks, m =>
        {
            Assert.NotNull(m.Label);
            Assert.NotNull(m.Page);
            Assert.NotNull(m.Description);
            Assert.StartsWith("/Operaciones/", m.Page);
        });
    }
}
