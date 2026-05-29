using System;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages.Admin.Dashboard;

[Authorize(Roles = "Administrador,Encargado,Gerente")]
public class DashboardModel : PageModel
{
    private readonly IMetricaServicio _metricaServicio;

    public DashboardModel(IMetricaServicio metricaServicio)
    {
        _metricaServicio = metricaServicio;
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
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    public async Task<IActionResult> OnGetVentasPorHoraJsonAsync()
    {
        try
        {
            var ventas = await _metricaServicio.ObtenerVentasPorHoraAsync();
            return new JsonResult(ventas);
        }
        catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
    }

    private async Task CargarDatosAsync()
    {
        try
        {
            Metricas = await _metricaServicio.ObtenerMetricasOperativasAsync();
            VentasPorHora = await _metricaServicio.ObtenerVentasPorHoraAsync();
        }
        catch (Exception ex) { ToastError = $"Error al cargar dashboard: {ex.Message}"; }
    }

    private void SetUiContext()
    {
        if (ViewData is not null)
        {
            ViewData["ActiveTab"] = "Dashboard";
        }
    }
}
