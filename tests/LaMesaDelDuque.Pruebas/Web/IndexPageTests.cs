using LaMesaDelDuque.Web.Pages;

namespace LaMesaDelDuque.Pruebas.Web;

public class IndexPageTests
{
    [Fact]
    public void OnGet_sets_pedidos_as_minimum_module()
    {
        var page = new IndexModel();

        page.OnGet();

        Assert.NotNull(page.ModuleLinks);
        Assert.True(page.ModuleLinks.Count >= 1); // al menos Pedidos
        Assert.Contains(page.ModuleLinks, m => m.Label == "Pedidos");
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
        });
    }
}
