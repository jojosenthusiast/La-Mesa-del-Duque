using LaMesaDelDuque.Aplicacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace LaMesaDelDuque.Web.Pages.Operaciones.Cierre;

[Authorize(Roles = "Administrador,Encargado")]
public class IndexModel : PageModel
{
    private readonly ICierreServicio _cierre;
    public CierreDiaDto? CierreAbierto { get; set; }
    public List<CierreDiaDto> Historial { get; set; } = [];

    [TempData] public string? ToastError { get; set; }

    public IndexModel(ICierreServicio cierre) => _cierre = cierre;

    public async Task OnGetAsync()
    {
        CierreAbierto = await _cierre.ObtenerCierreHoyAsync();
        Historial = await _cierre.HistorialAsync();
    }

    public async Task<IActionResult> OnPostAbrirAsync()
    {
        var usuarioId = GetUsuarioId();
        if (usuarioId == Guid.Empty)
        {
            ToastError = "No se pudo identificar el usuario autenticado.";
            return RedirectToPage();
        }
        await _cierre.AbrirCierreAsync(usuarioId);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCerrarAsync(CierreCajaRequest req)
    {
        var usuarioId = GetUsuarioId();
        if (usuarioId == Guid.Empty)
        {
            ToastError = "No se pudo identificar el usuario autenticado.";
            return RedirectToPage();
        }
        await _cierre.CerrarDiaAsync(req, usuarioId);
        return RedirectToPage();
    }

    private Guid GetUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
