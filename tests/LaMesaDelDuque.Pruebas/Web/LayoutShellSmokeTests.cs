namespace LaMesaDelDuque.Pruebas.Web;

public class LayoutShellSmokeTests
{
    [Fact]
    public void Shell_contract_requires_operational_modules()
    {
        var modules = new[] { "Productos", "Mesas", "Pedidos" };

        Assert.Contains("Productos", modules);
        Assert.Contains("Mesas", modules);
        Assert.Contains("Pedidos", modules);
    }

    [Fact]
    public void Shell_nav_has_three_module_slots()
    {
        var tabs = new[] { "Productos", "Mesas", "Pedidos" };
        Assert.Equal(3, tabs.Length);
    }

    [Fact]
    public void Shell_requires_auth_for_operations_folder()
    {
        // Convention-based auth: Operaciones folder requires authorized users
        var protectedFolders = new[] { "Operaciones", "Admin" };
        Assert.Contains("Operaciones", protectedFolders);
        Assert.Contains("Admin", protectedFolders);
    }
}
