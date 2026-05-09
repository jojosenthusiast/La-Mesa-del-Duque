using LaMesaDelDuque.Web.Pages;

namespace LaMesaDelDuque.Pruebas.Web;

public class IndexPageTests
{
    [Fact]
    public void OnGet_returns_empty_modules_when_not_authenticated()
    {
        var page = new IndexModel();

        page.OnGet();

        Assert.NotNull(page.ModuleLinks);
        Assert.Empty(page.ModuleLinks); // sin autenticación, sin módulos
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
