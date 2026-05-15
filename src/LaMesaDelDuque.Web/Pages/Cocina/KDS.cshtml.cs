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

    public static IReadOnlyList<CookConfig> Cooks { get; } = new List<CookConfig>
    {
        new(1, "Cocinero 1", "#e74c3c"),
        new(2, "Cocinero 2", "#3498db"),
        new(3, "Cocinero 3", "#2ecc71")
    };

    public static IReadOnlyDictionary<string, int> StationToColumn { get; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        { "Parrilla", 1 },
        { "Fria", 2 },
        { "Caliente", 3 },
        { "Bar", 2 },
        { "Expo", 1 }
    };

    public record CookConfig(int Id, string Name, string Color);

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
