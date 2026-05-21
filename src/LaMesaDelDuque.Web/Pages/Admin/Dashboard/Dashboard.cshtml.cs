using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages.Admin.Dashboard;

[Authorize(Roles = "Administrador,Encargado")]
public class DashboardModel : PageModel
{
    private readonly IMetricaServicio _metricaServicio;

    public DashboardModel(IMetricaServicio metricaServicio)
    {
        _metricaServicio = metricaServicio;
    }

    public MetricasOperativasDto Metricas { get; set; } = new();
    public List<VentaPorHoraDto> VentasPorHora { get; set; } = new();

    public async Task OnGetAsync()
    {
        SetUiContext();
        await CargarDatosAsync();
    }

    public async Task<IActionResult> OnGetMetricasJsonAsync()
    {
        var metricas = await _metricaServicio.ObtenerMetricasOperativasAsync();
        return new JsonResult(metricas);
    }

    public async Task<IActionResult> OnGetVentasPorHoraJsonAsync()
    {
        var ventas = await _metricaServicio.ObtenerVentasPorHoraAsync();
        return new JsonResult(ventas);
    }

    private async Task CargarDatosAsync()
    {
        Metricas = await _metricaServicio.ObtenerMetricasOperativasAsync();
        VentasPorHora = await _metricaServicio.ObtenerVentasPorHoraAsync();
    }

    private void SetUiContext()
    {
        if (ViewData is not null)
        {
            ViewData["ActiveTab"] = "Dashboard";
        }
    }
}
