using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages.Operaciones.TurnoCaja;

[Authorize(Roles = "Administrador,Encargado,Gerente,Cajero")]
public class HistorialModel : PageModel
{
    private readonly ITurnoCajaServicio _servicio;
    private readonly ILogger<HistorialModel> _logger;

    public List<TurnoCajaDto> Turnos { get; set; } = [];
    public ReporteZDto? ReporteZ { get; set; }

    [TempData] public string? ToastError { get; set; }

    [BindProperty(SupportsGet = true)] public int Pagina { get; set; } = 1;
    public const int PorPagina = 20;

    public HistorialModel(ITurnoCajaServicio servicio, ILogger<HistorialModel> logger)
    {
        _servicio = servicio;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        Turnos = await _servicio.ObtenerHistorialAsync(Pagina, PorPagina);
    }

    public async Task<IActionResult> OnGetReporteZAsync(Guid turnoId)
    {
        try
        {
            var reporte = await _servicio.GenerarReporteZAsync(turnoId);
            return new JsonResult(reporte);
        }
        catch (ReglaDominioException ex)
        {
            return StatusCode(422, new { ok = false, error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar Reporte Z");
            return StatusCode(500, new { ok = false, error = "Error interno." });
        }
    }
}
