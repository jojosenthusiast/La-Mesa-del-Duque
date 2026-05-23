using LaMesaDelDuque.Aplicacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages.Operaciones.Inventario;

[Authorize(Roles = "Administrador,Encargado")]
public class IndexModel : PageModel
{
    private readonly IInventarioServicio _inv;
    private readonly IMermaServicio _merma;
    public List<IngredienteDto> Ingredientes { get; set; } = [];
    public List<ProveedorDto> Proveedores { get; set; } = [];
    public List<MermaDiariaDto> Mermas { get; set; } = [];

    public IndexModel(IInventarioServicio inv, IMermaServicio merma) { _inv = inv; _merma = merma; }

    public async Task OnGetAsync()
    {
        Ingredientes = await _inv.ListarIngredientesAsync();
        Proveedores = await _inv.ListarProveedoresAsync();
        Mermas = await _merma.ObtenerMermasDelDiaAsync();
    }
}
