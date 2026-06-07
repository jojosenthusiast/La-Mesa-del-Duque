using LaMesaDelDuque.Aplicacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages.Admin.Reportes;

[Authorize(Roles = "Administrador,Encargado,Gerente")]
public class IndexModel : PageModel
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly IReportesServicio _reportes;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IReportesServicio reportes, ILogger<IndexModel> logger)
    {
        _reportes = reportes;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public DateTime Desde { get; set; } = new(DateTime.Today.Year, DateTime.Today.Month, 1);

    [BindProperty(SupportsGet = true)]
    public DateTime Hasta { get; set; } = DateTime.Today;

    [TempData]
    public string? ToastError { get; set; }

    public void OnGet()
    {
        SetUiContext();
    }

    public async Task<IActionResult> OnGetVentasPdfAsync(DateTime desde, DateTime hasta)
    {
        if (!RangoValido(desde, hasta))
        {
            return Page();
        }

        var (desdeNormalizado, hastaNormalizado) = NormalizarRango(desde, hasta);
        try
        {
            var bytes = await _reportes.GenerarReporteVentasPdfAsync(desdeNormalizado, hastaNormalizado);
            return File(bytes, "application/pdf", $"ventas_{desde:yyyyMMdd}_{hasta:yyyyMMdd}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo generar el reporte de ventas PDF para el rango {Desde} - {Hasta}.", desde, hasta);
            PrepararError(desde, hasta, "No se pudo generar el reporte de ventas en PDF.");
            return Page();
        }
    }

    public async Task<IActionResult> OnGetVentasExcelAsync(DateTime desde, DateTime hasta)
    {
        if (!RangoValido(desde, hasta))
        {
            return Page();
        }

        var (desdeNormalizado, hastaNormalizado) = NormalizarRango(desde, hasta);
        try
        {
            var bytes = await _reportes.GenerarReporteVentasExcelAsync(desdeNormalizado, hastaNormalizado);
            return File(bytes, ExcelContentType, $"ventas_{desde:yyyyMMdd}_{hasta:yyyyMMdd}.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo generar el reporte de ventas Excel para el rango {Desde} - {Hasta}.", desde, hasta);
            PrepararError(desde, hasta, "No se pudo generar el reporte de ventas en Excel.");
            return Page();
        }
    }

    public async Task<IActionResult> OnGetKardexExcelAsync(DateTime desde, DateTime hasta)
    {
        if (!RangoValido(desde, hasta))
        {
            return Page();
        }

        var (desdeNormalizado, hastaNormalizado) = NormalizarRango(desde, hasta);
        try
        {
            var bytes = await _reportes.GenerarKardexExcelAsync(desdeNormalizado, hastaNormalizado);
            return File(bytes, ExcelContentType, $"kardex_{desde:yyyyMMdd}_{hasta:yyyyMMdd}.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo generar el Kardex Excel para el rango {Desde} - {Hasta}.", desde, hasta);
            PrepararError(desde, hasta, "No se pudo generar el Kardex en Excel.");
            return Page();
        }
    }

    public async Task<IActionResult> OnGetMermasPdfAsync(DateTime desde, DateTime hasta)
    {
        if (!RangoValido(desde, hasta))
        {
            return Page();
        }

        var (desdeNormalizado, hastaNormalizado) = NormalizarRango(desde, hasta);
        try
        {
            var bytes = await _reportes.GenerarReporteMermasPdfAsync(desdeNormalizado, hastaNormalizado);
            return File(bytes, "application/pdf", $"mermas_{desde:yyyyMMdd}_{hasta:yyyyMMdd}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo generar el reporte de mermas PDF para el rango {Desde} - {Hasta}.", desde, hasta);
            PrepararError(desde, hasta, "No se pudo generar el reporte de mermas en PDF.");
            return Page();
        }
    }

    private bool RangoValido(DateTime desde, DateTime hasta)
    {
        if (hasta.Date >= desde.Date)
        {
            return true;
        }

        PrepararError(desde, hasta, "El rango de fechas no es válido. La fecha Hasta debe ser igual o posterior a Desde.");
        return false;
    }

    private void PrepararError(DateTime desde, DateTime hasta, string mensaje)
    {
        Desde = desde.Date;
        Hasta = hasta.Date;
        ToastError = mensaje;
        SetUiContext();
    }

    private static (DateTime Desde, DateTime Hasta) NormalizarRango(DateTime desde, DateTime hasta) =>
        (desde.Date, hasta.Date.AddDays(1).AddTicks(-1));

    private void SetUiContext()
    {
        ViewData["Title"] = "Reportes";
        ViewData["ActiveTab"] = "Reportes";
    }
}
