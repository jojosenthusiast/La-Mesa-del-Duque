using LaMesaDelDuque.Aplicacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages.Operaciones.Cierre;

[Authorize(Roles = "Administrador,Encargado")]
public class IndexModel : PageModel
{
    private readonly ICierreServicio _cierre;
    public CierreDiaDto? CierreAbierto { get; set; }
    public List<CierreDiaDto> Historial { get; set; } = [];

    public IndexModel(ICierreServicio cierre) => _cierre = cierre;

    public async Task OnGetAsync()
    {
        CierreAbierto = await _cierre.ObtenerCierreHoyAsync();
        Historial = await _cierre.HistorialAsync();
    }

    public async Task<IActionResult> OnPostAbrirAsync()
    {
        await _cierre.AbrirCierreAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCerrarAsync(CierreCajaRequest req)
    {
        await _cierre.CerrarDiaAsync(req);
        return RedirectToPage();
    }
}
