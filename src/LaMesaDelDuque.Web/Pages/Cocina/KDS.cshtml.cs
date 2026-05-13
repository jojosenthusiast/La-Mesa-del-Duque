using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Enumeraciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages.Cocina;

[Authorize(Roles = "Cocinero,Encargado,Administrador")]
public class KDSModel : PageModel
{
    private readonly ICocinaServicio _cocinaServicio;

    public KDSModel(ICocinaServicio cocinaServicio)
    {
        _cocinaServicio = cocinaServicio;
    }

    public List<OrdenCocinaDto> Ordenes { get; set; } = [];

    public async Task OnGetAsync()
    {
        Ordenes = await _cocinaServicio.ListarPendientesAsync();
    }

    public async Task<IActionResult> OnGetOrdenesJsonAsync(string estacion)
    {
        EstacionCocina? filtro = null;
        if (!string.IsNullOrWhiteSpace(estacion) && estacion != "Todas" && Enum.TryParse<EstacionCocina>(estacion, out var estacionEnum))
        {
            filtro = estacionEnum;
        }

        var ordenes = await _cocinaServicio.ListarPendientesAsync(filtro);
        return new JsonResult(ordenes);
    }

    public async Task<IActionResult> OnPostMarcarListoJsonAsync(Guid ordenId)
    {
        try
        {
            var dto = await _cocinaServicio.MarcarListoAsync(ordenId);
            return new JsonResult(dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
