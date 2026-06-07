using LaMesaDelDuque.Aplicacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages.Admin.Reportes;

[Authorize(Roles = "Administrador,Encargado,Gerente")]
public class IndexModel : PageModel
{
    private readonly IReportesServicio _reportes;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IReportesServicio reportes, ILogger<IndexModel> logger)
    {
        _reportes = reportes;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public DateTime Desde { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

    [BindProperty(SupportsGet = true)]
    public DateTime Hasta { get; set; } = DateTime.Today;

    [TempData] public string? ToastError { get; set; }

    public void OnGet()
    {
        ViewData["Title"] = "Reportes";
        ViewData["ActiveTab"] = "Reportes";
    }

    public async Task<IActionResult> OnGetVentasExcelAsync(DateTime desde, DateTime hasta)
    {
        try
        {
            var bytes = await _reportes.GenerarReporteVentasExcelAsync(
                desde.Date, hasta.Date.AddDays(1).AddSeconds(-1));
            var nombre = $"ventas_{desde:yyyyMMdd}_{hasta:yyyyMMdd}.xlsx";
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                nombre);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando reporte ventas Excel");
            ToastError = "Error al generar el reporte Excel de ventas.";
            return RedirectToPage(new { desde, hasta });
        }
    }

    public async Task<IActionResult> OnGetVentasPdfAsync(DateTime desde, DateTime hasta)
    {
        try
        {
            var bytes = await _reportes.GenerarReporteVentasPdfAsync(
                desde.Date, hasta.Date.AddDays(1).AddSeconds(-1));
            var nombre = $"ventas_{desde:yyyyMMdd}_{hasta:yyyyMMdd}.pdf";
            return File(bytes, "application/pdf", nombre);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando reporte ventas PDF");
            ToastError = "Error al generar el reporte PDF de ventas.";
            return RedirectToPage(new { desde, hasta });
        }
    }

    public async Task<IActionResult> OnGetKardexExcelAsync(DateTime desde, DateTime hasta)
    {
        try
        {
            var bytes = await _reportes.GenerarKardexExcelAsync(
                desde.Date, hasta.Date.AddDays(1).AddSeconds(-1));
            var nombre = $"kardex_{desde:yyyyMMdd}_{hasta:yyyyMMdd}.xlsx";
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                nombre);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando Kardex Excel");
            ToastError = "Error al generar el Kardex Excel.";
            return RedirectToPage(new { desde, hasta });
        }
    }

    public async Task<IActionResult> OnGetMermasPdfAsync(DateTime desde, DateTime hasta)
    {
        try
        {
            var bytes = await _reportes.GenerarReporteMermasPdfAsync(
                desde.Date, hasta.Date.AddDays(1).AddSeconds(-1));
            var nombre = $"mermas_{desde:yyyyMMdd}_{hasta:yyyyMMdd}.pdf";
            return File(bytes, "application/pdf", nombre);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando reporte mermas PDF");
            ToastError = "Error al generar el reporte PDF de mermas.";
            return RedirectToPage(new { desde, hasta });
        }
    }
}
