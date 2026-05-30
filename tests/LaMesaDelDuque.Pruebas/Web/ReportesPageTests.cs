using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Web.Pages.Admin.Reportes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;

namespace LaMesaDelDuque.Pruebas.Web;

public class ReportesPageTests
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    [Fact]
    public void IndexModel_DebePermitirSoloRolesGerenciales()
    {
        var attribute = typeof(IndexModel)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .OfType<AuthorizeAttribute>()
            .Single();

        var roles = attribute.Roles?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [];

        Assert.Contains("Administrador", roles);
        Assert.Contains("Encargado", roles);
        Assert.Contains("Gerente", roles);
        Assert.DoesNotContain("Cajero", roles);
        Assert.DoesNotContain("Mesero", roles);
    }

    [Fact]
    public void OnGet_DebeConfigurarTituloYFechasPorDefecto()
    {
        var page = CreatePage(new FakeReportesServicio());
        var expectedDesde = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        page.OnGet();

        Assert.Equal("Reportes", page.ViewData["Title"]);
        Assert.Equal("Reportes", page.ViewData["ActiveTab"]);
        Assert.Equal(expectedDesde, page.Desde);
        Assert.Equal(DateTime.Today, page.Hasta);
    }

    [Fact]
    public async Task OnGetVentasPdfAsync_DebeRetornarPdfConRangoInclusivo()
    {
        var servicio = new FakeReportesServicio();
        var page = CreatePage(servicio);
        var desde = new DateTime(2026, 5, 1);
        var hasta = new DateTime(2026, 5, 3);

        var result = await page.OnGetVentasPdfAsync(desde, hasta);

        var file = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", file.ContentType);
        Assert.Equal("ventas_20260501_20260503.pdf", file.FileDownloadName);
        Assert.Equal(FakeReportesServicio.VentasPdfBytes, file.FileContents);
        Assert.Equal("VentasPdf", servicio.LastMethod);
        Assert.Equal(desde.Date, servicio.LastDesde);
        Assert.Equal(hasta.Date.AddDays(1).AddTicks(-1), servicio.LastHasta);
    }

    [Fact]
    public async Task OnGetVentasExcelAsync_DebeRetornarExcelConNombreDescargable()
    {
        var servicio = new FakeReportesServicio();
        var page = CreatePage(servicio);
        var desde = new DateTime(2026, 5, 1);
        var hasta = new DateTime(2026, 5, 3);

        var result = await page.OnGetVentasExcelAsync(desde, hasta);

        var file = Assert.IsType<FileContentResult>(result);
        Assert.Equal(ExcelContentType, file.ContentType);
        Assert.Equal("ventas_20260501_20260503.xlsx", file.FileDownloadName);
        Assert.Equal(FakeReportesServicio.VentasExcelBytes, file.FileContents);
        Assert.Equal("VentasExcel", servicio.LastMethod);
    }

    [Fact]
    public async Task OnGetKardexExcelAsync_DebeRetornarExcelConNombreDescargable()
    {
        var servicio = new FakeReportesServicio();
        var page = CreatePage(servicio);
        var desde = new DateTime(2026, 5, 1);
        var hasta = new DateTime(2026, 5, 3);

        var result = await page.OnGetKardexExcelAsync(desde, hasta);

        var file = Assert.IsType<FileContentResult>(result);
        Assert.Equal(ExcelContentType, file.ContentType);
        Assert.Equal("kardex_20260501_20260503.xlsx", file.FileDownloadName);
        Assert.Equal(FakeReportesServicio.KardexExcelBytes, file.FileContents);
        Assert.Equal("KardexExcel", servicio.LastMethod);
    }

    [Fact]
    public async Task OnGetMermasPdfAsync_DebeRetornarPdfConNombreDescargable()
    {
        var servicio = new FakeReportesServicio();
        var page = CreatePage(servicio);
        var desde = new DateTime(2026, 5, 1);
        var hasta = new DateTime(2026, 5, 3);

        var result = await page.OnGetMermasPdfAsync(desde, hasta);

        var file = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", file.ContentType);
        Assert.Equal("mermas_20260501_20260503.pdf", file.FileDownloadName);
        Assert.Equal(FakeReportesServicio.MermasPdfBytes, file.FileContents);
        Assert.Equal("MermasPdf", servicio.LastMethod);
    }

    [Fact]
    public async Task Handler_ConRangoInvalido_DebeRetornarPageSinLlamarServicio()
    {
        var servicio = new FakeReportesServicio();
        var page = CreatePage(servicio);

        var result = await page.OnGetVentasPdfAsync(new DateTime(2026, 5, 5), new DateTime(2026, 5, 1));

        Assert.IsType<PageResult>(result);
        Assert.Equal(0, servicio.CallCount);
        Assert.Contains("rango", page.ToastError, StringComparison.OrdinalIgnoreCase);
    }

    private static IndexModel CreatePage(IReportesServicio servicio)
    {
        var page = new IndexModel(servicio, NullLogger<IndexModel>.Instance);
        page.PageContext = new PageContext
        {
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        };

        return page;
    }

    private sealed class FakeReportesServicio : IReportesServicio
    {
        public static readonly byte[] VentasPdfBytes = [1, 2, 3];
        public static readonly byte[] VentasExcelBytes = [4, 5, 6];
        public static readonly byte[] KardexExcelBytes = [7, 8, 9];
        public static readonly byte[] MermasPdfBytes = [10, 11, 12];

        public string? LastMethod { get; private set; }
        public DateTime? LastDesde { get; private set; }
        public DateTime? LastHasta { get; private set; }
        public int CallCount { get; private set; }

        public Task<byte[]> GenerarReporteVentasPdfAsync(DateTime desde, DateTime hasta, CancellationToken ct = default) =>
            Return("VentasPdf", desde, hasta, VentasPdfBytes);

        public Task<byte[]> GenerarReporteVentasExcelAsync(DateTime desde, DateTime hasta, CancellationToken ct = default) =>
            Return("VentasExcel", desde, hasta, VentasExcelBytes);

        public Task<byte[]> GenerarKardexExcelAsync(DateTime desde, DateTime hasta, CancellationToken ct = default) =>
            Return("KardexExcel", desde, hasta, KardexExcelBytes);

        public Task<byte[]> GenerarReporteMermasPdfAsync(DateTime desde, DateTime hasta, CancellationToken ct = default) =>
            Return("MermasPdf", desde, hasta, MermasPdfBytes);

        private Task<byte[]> Return(string method, DateTime desde, DateTime hasta, byte[] bytes)
        {
            LastMethod = method;
            LastDesde = desde;
            LastHasta = hasta;
            CallCount++;
            return Task.FromResult(bytes);
        }
    }
}
