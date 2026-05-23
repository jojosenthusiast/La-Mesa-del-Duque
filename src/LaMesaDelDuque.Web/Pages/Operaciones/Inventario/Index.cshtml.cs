using LaMesaDelDuque.Aplicacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages.Operaciones.Inventario;

[Authorize(Roles = "Administrador,Encargado")]
public class IndexModel : PageModel
{
    private readonly IInventarioServicio _inv;
    public List<IngredienteDto> Ingredientes { get; set; } = [];
    public List<ProveedorDto> Proveedores { get; set; } = [];

    public IndexModel(IInventarioServicio inv) => _inv = inv;

    public async Task OnGetAsync()
    {
        Ingredientes = await _inv.ListarIngredientesAsync();
        Proveedores = await _inv.ListarProveedoresAsync();
    }
}
