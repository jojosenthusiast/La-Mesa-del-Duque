using LaMesaDelDuque.Aplicacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LaMesaDelDuque.Web.Pages.Operaciones.Cierre;

[Authorize(Roles = "Administrador,Encargado,Cajero")]
public class IndexModel : PageModel
{
    private readonly ICierreServicio _cierre;
    public CierreDiaDto? CierreAbierto { get; set; }
    public List<CierreDiaDto> Historial { get; set; } = [];
    public List<PersonalJornadaDto> Personal { get; set; } = [];
    public List<CanalVentaResumenDto> CanalesHoy { get; set; } = [];

    [TempData] public string? ToastError { get; set; }

    public IndexModel(ICierreServicio cierre) => _cierre = cierre;

    public async Task OnGetAsync()
    {
        CierreAbierto = await _cierre.ObtenerCierreHoyAsync();
        Historial = await _cierre.HistorialAsync();
        Personal = await _cierre.PersonalDeLaJornadaAsync(DateOnly.FromDateTime(DateTime.UtcNow));
        CanalesHoy = await _cierre.TotalesPorCanalAsync(DateOnly.FromDateTime(DateTime.UtcNow));
    }

    public async Task<IActionResult> OnGetExportarAsync(CancellationToken ct)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var cierre = await _cierre.ObtenerCierreHoyAsync(ct);
        var historial = await _cierre.HistorialAsync(ct);
        var cierreHoy = cierre ?? historial.FirstOrDefault(h => h.Fecha == hoy);
        var personal = await _cierre.PersonalDeLaJornadaAsync(hoy, ct);
        var canales = await _cierre.TotalesPorCanalAsync(hoy, ct);

        var bytes = Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(t => t.FontSize(10));

                page.Header().Column(h =>
                {
                    h.Item().Text("La Mesa del Duque").FontSize(16).Bold();
                    h.Item().Text($"Reporte de cierre del día — {hoy:dd/MM/yyyy}").FontSize(11);
                });

                page.Content().PaddingVertical(12).Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Text("Totales del día").Bold().FontSize(12);
                    col.Item().Text($"Ventas totales: {(cierreHoy?.TotalVentas ?? 0):C}");
                    col.Item().Text($"Efectivo (sistema): {(cierreHoy?.TotalEfectivo ?? 0):C}   |   Efectivo (real): {(cierreHoy?.EfectivoReal ?? 0):C}");
                    col.Item().Text($"Tarjeta/QR/Transf. (sistema): {(cierreHoy?.TotalTarjeta ?? 0):C}   |   Tarjeta/QR/Transf. (real): {(cierreHoy?.TarjetaReal ?? 0):C}");
                    col.Item().Text($"Pedidos: {(cierreHoy?.TotalPedidos ?? 0)}   |   Cancelados: {(cierreHoy?.Cancelados ?? 0)}   |   Merma: {(cierreHoy?.TotalMerma ?? 0):C}");
                    col.Item().Text($"Estado: {(cierreHoy?.EsCerrado == true ? "Cerrado" : "Abierto")}");

                    col.Item().PaddingTop(8).Text("Ventas por canal").Bold().FontSize(12);
                    if (canales.Count == 0)
                    {
                        col.Item().Text("Sin ventas por canal para hoy.").Italic();
                    }
                    else
                    {
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(1);
                                c.RelativeColumn(1);
                                c.RelativeColumn(2);
                            });
                            t.Header(header =>
                            {
                                header.Cell().Text("Canal").Bold();
                                header.Cell().Text("Pedidos").Bold();
                                header.Cell().Text("Pagados").Bold();
                                header.Cell().Text("Total vendido").Bold();
                            });
                            foreach (var c in canales)
                            {
                                t.Cell().Text(c.Nombre);
                                t.Cell().Text(c.Pedidos.ToString());
                                t.Cell().Text(c.Pagados.ToString());
                                t.Cell().Text(c.TotalVendido.ToString("C"));
                            }
                        });
                    }

                    col.Item().PaddingTop(8).Text("Personal de la jornada").Bold().FontSize(12);

                    if (personal.Count == 0)
                    {
                        col.Item().Text("Sin registros de personal para hoy.").Italic();
                    }
                    else
                    {
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                            });
                            t.Header(header =>
                            {
                                header.Cell().Text("Nombre").Bold();
                                header.Cell().Text("Rol").Bold();
                                header.Cell().Text("Entrada").Bold();
                                header.Cell().Text("Salida").Bold();
                            });
                            foreach (var p in personal)
                            {
                                t.Cell().Text(p.NombreCompleto);
                                t.Cell().Text(p.Rol);
                                t.Cell().Text(p.Entrada.HasValue ? p.Entrada.Value.ToLocalTime().ToString("HH:mm") : "—");
                                t.Cell().Text(p.Salida.HasValue ? p.Salida.Value.ToLocalTime().ToString("HH:mm") : "—");
                            }
                        });
                    }
                });

                page.Footer().AlignRight().Text(t =>
                {
                    t.Span("Generado: ");
                    t.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                });
            });
        }).GeneratePdf();

        return File(bytes, "application/pdf", $"cierre-{hoy:yyyyMMdd}.pdf");
    }

    public async Task<IActionResult> OnPostAbrirAsync()
    {
        var usuarioId = GetUsuarioId();
        if (usuarioId == Guid.Empty)
        {
            ToastError = "No se pudo identificar el usuario autenticado.";
            return RedirectToPage();
        }
        try
        {
            await _cierre.AbrirCierreAsync(usuarioId);
        }
        catch (Exception ex)
        {
            ToastError = ex.Message;
        }
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
        try
        {
            await _cierre.CerrarDiaAsync(req, usuarioId);
        }
        catch (Exception ex)
        {
            ToastError = ex.Message;
        }
        return RedirectToPage();
    }

    private Guid GetUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
