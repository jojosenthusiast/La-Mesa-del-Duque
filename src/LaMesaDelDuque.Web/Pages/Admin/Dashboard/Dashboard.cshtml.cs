using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Dominio.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages.Admin.Dashboard;

[Authorize(Roles = "Administrador,Encargado,Gerente")]
public class DashboardModel : PageModel
{
    private readonly IMetricaServicio _metricaServicio;
    private readonly ILogger<DashboardModel> _logger;

    public DashboardModel(IMetricaServicio metricaServicio, ILogger<DashboardModel> logger)
    {
        _metricaServicio = metricaServicio;
        _logger = logger;
    }

    public MetricasOperativasDto Metricas { get; set; } = new();
    public List<VentaPorHoraDto> VentasPorHora { get; set; } = new();

    [TempData] public string? ToastError { get; set; }

    public async Task OnGetAsync()
    {
        SetUiContext();
        await CargarDatosAsync();
    }

    public async Task<IActionResult> OnGetMetricasJsonAsync()
    {
        try
        {
            var metricas = await _metricaServicio.ObtenerMetricasOperativasAsync();
            return new JsonResult(metricas);
        }
        catch (ReglaDominioException ex) { return StatusCode(422, new { ok = false, error = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error en handler JSON"); return StatusCode(500, new { ok = false, error = "Ocurrió un error interno." }); }
    }

    public async Task<IActionResult> OnGetVentasPorHoraJsonAsync()
    {
        try
        {
            var ventas = await _metricaServicio.ObtenerVentasPorHoraAsync();
            return new JsonResult(ventas);
        }
        catch (ReglaDominioException ex) { return StatusCode(422, new { ok = false, error = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error en handler JSON"); return StatusCode(500, new { ok = false, error = "Ocurrió un error interno." }); }
    }

    private async Task CargarDatosAsync()
    {
        try
        {
            Metricas = await _metricaServicio.ObtenerMetricasOperativasAsync();
            VentasPorHora = await _metricaServicio.ObtenerVentasPorHoraAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al cargar dashboard administrativo.");
            ToastError = "No se pudo cargar el dashboard. Intenta nuevamente.";
        }
    }

    private void SetUiContext()
    {
        if (ViewData is not null)
        {
            ViewData["ActiveTab"] = "Dashboard";
        }
    }
}
