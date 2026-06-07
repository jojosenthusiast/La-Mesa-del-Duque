using ClosedXML.Excel;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Repositorios;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface IReportesServicio
{
    Task<byte[]> GenerarReporteVentasPdfAsync(DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<byte[]> GenerarReporteVentasExcelAsync(DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<byte[]> GenerarKardexExcelAsync(DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<byte[]> GenerarReporteMermasPdfAsync(DateTime desde, DateTime hasta, CancellationToken ct = default);
}

public class ReportesServicio : IReportesServicio
{
    private readonly IUnidadDeTrabajo _uot;

    public ReportesServicio(IUnidadDeTrabajo uot) => _uot = uot;

    // ── Ventas Excel ──────────────────────────────────────────────────────────

    public async Task<byte[]> GenerarReporteVentasExcelAsync(
        DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        var pedidos = await ObtenerPedidosPagadosAsync(desde, hasta, ct);
        var pagos = await _uot.Pagos.ObtenerPorRangoFechaAsync(desde, hasta, ct);

        var usuarioIds = pagos.Select(p => p.UsuarioId).Distinct().ToList();
        var usuarios = new Dictionary<Guid, string>();
        foreach (var uid in usuarioIds)
        {
            var u = await _uot.Usuarios.ObtenerPorIdAsync(uid, ct);
            if (u is not null) usuarios[uid] = u.NombreCompleto;
        }

        var pagosPorMetodo = pagos
            .GroupBy(p => p.Metodo)
            .Select(g => new { Metodo = g.Key.ToString(), Count = g.Count(), Total = g.Sum(x => x.Monto) })
            .OrderByDescending(x => x.Total)
            .ToList();

        using var wb = new XLWorkbook();

        // Hoja 1: Detalle Ventas
        var wsD = wb.Worksheets.Add("Detalle Ventas");
        var hd = new[] { "Fecha", "Mesa", "Producto", "Cantidad", "Precio Unit.", "Descuento", "Subtotal" };
        for (int i = 0; i < hd.Length; i++)
        {
            var c = wsD.Cell(1, i + 1);
            c.Value = hd[i];
            c.Style.Font.Bold = true;
            c.Style.Fill.BackgroundColor = XLColor.FromHtml("#1a1a2e");
            c.Style.Font.FontColor = XLColor.FromHtml("#d4af37");
        }

        int rD = 2;
        foreach (var p in pedidos)
        {
            var mesa = p.Mesa is not null ? $"Mesa {p.Mesa.Numero}" : "Para llevar";
            foreach (var det in p.Detalles)
            {
                wsD.Cell(rD, 1).Value = p.FechaCreacion.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
                wsD.Cell(rD, 2).Value = mesa;
                wsD.Cell(rD, 3).Value = det.Producto.Nombre;
                wsD.Cell(rD, 4).Value = det.Cantidad;
                wsD.Cell(rD, 5).Value = det.PrecioUnitario;
                wsD.Cell(rD, 5).Style.NumberFormat.Format = "#,##0.00";
                wsD.Cell(rD, 6).Value = det.DescuentoAplicado;
                wsD.Cell(rD, 6).Style.NumberFormat.Format = "#,##0.00";
                wsD.Cell(rD, 7).Value = det.Subtotal;
                wsD.Cell(rD, 7).Style.NumberFormat.Format = "#,##0.00";
                rD++;
            }
        }
        wsD.Columns().AdjustToContents();

        // Hoja 2: Resumen por Día
        var wsRD = wb.Worksheets.Add("Resumen por Dia");
        wsRD.Cell(1, 1).Value = "Fecha";
        wsRD.Cell(1, 2).Value = "Pedidos";
        wsRD.Cell(1, 3).Value = "Total Ventas";
        wsRD.Row(1).Style.Font.Bold = true;

        var porDia = pedidos
            .GroupBy(p => p.FechaCreacion.ToLocalTime().Date)
            .Select(g => new { Fecha = g.Key, Pedidos = g.Count(), Total = g.Sum(p => p.Total) })
            .OrderBy(x => x.Fecha)
            .ToList();

        int rRD = 2;
        foreach (var d in porDia)
        {
            wsRD.Cell(rRD, 1).Value = d.Fecha.ToString("yyyy-MM-dd");
            wsRD.Cell(rRD, 2).Value = d.Pedidos;
            wsRD.Cell(rRD, 3).Value = d.Total;
            wsRD.Cell(rRD, 3).Style.NumberFormat.Format = "#,##0.00";
            rRD++;
        }
        wsRD.Cell(rRD, 1).Value = "TOTAL";
        wsRD.Cell(rRD, 1).Style.Font.Bold = true;
        wsRD.Cell(rRD, 2).Value = pedidos.Count;
        wsRD.Cell(rRD, 3).Value = porDia.Sum(d => d.Total);
        wsRD.Cell(rRD, 3).Style.NumberFormat.Format = "#,##0.00";
        wsRD.Cell(rRD, 3).Style.Font.Bold = true;
        wsRD.Columns().AdjustToContents();

        // Hoja 3: Por Producto
        var wsPr = wb.Worksheets.Add("Por Producto");
        wsPr.Cell(1, 1).Value = "Producto";
        wsPr.Cell(1, 2).Value = "Cantidad";
        wsPr.Cell(1, 3).Value = "Total";
        wsPr.Row(1).Style.Font.Bold = true;

        var porProd = pedidos.SelectMany(p => p.Detalles)
            .GroupBy(d => d.Producto.Nombre)
            .Select(g => new { Producto = g.Key, Cantidad = g.Sum(d => d.Cantidad), Total = g.Sum(d => d.Subtotal) })
            .OrderByDescending(x => x.Total)
            .ToList();

        int rPr = 2;
        foreach (var pr in porProd)
        {
            wsPr.Cell(rPr, 1).Value = pr.Producto;
            wsPr.Cell(rPr, 2).Value = pr.Cantidad;
            wsPr.Cell(rPr, 3).Value = pr.Total;
            wsPr.Cell(rPr, 3).Style.NumberFormat.Format = "#,##0.00";
            rPr++;
        }
        wsPr.Columns().AdjustToContents();

        // Hoja 4: Por Metodo de Pago
        var wsM = wb.Worksheets.Add("Por Metodo de Pago");
        wsM.Cell(1, 1).Value = "Metodo";
        wsM.Cell(1, 2).Value = "Pagos";
        wsM.Cell(1, 3).Value = "Total Cobrado";
        wsM.Cell(1, 4).Value = "Cajeros";
        wsM.Row(1).Style.Font.Bold = true;

        int rM = 2;
        foreach (var met in pagosPorMetodo)
        {
            var cajeros = pagos
                .Where(pa => pa.Metodo.ToString() == met.Metodo)
                .Select(pa => usuarios.TryGetValue(pa.UsuarioId, out var n) ? n : pa.UsuarioId.ToString())
                .Distinct()
                .ToList();

            wsM.Cell(rM, 1).Value = met.Metodo;
            wsM.Cell(rM, 2).Value = met.Count;
            wsM.Cell(rM, 3).Value = met.Total;
            wsM.Cell(rM, 3).Style.NumberFormat.Format = "#,##0.00";
            wsM.Cell(rM, 4).Value = string.Join(", ", cajeros);
            rM++;
        }
        wsM.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ── Ventas PDF ────────────────────────────────────────────────────────────

    public async Task<byte[]> GenerarReporteVentasPdfAsync(
        DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        var pedidos = await ObtenerPedidosPagadosAsync(desde, hasta, ct);
        var pagos = await _uot.Pagos.ObtenerPorRangoFechaAsync(desde, hasta, ct);

        var porDia = pedidos
            .GroupBy(p => p.FechaCreacion.ToLocalTime().Date)
            .Select(g => new { Fecha = g.Key, Count = g.Count(), Total = g.Sum(p => p.Total) })
            .OrderBy(x => x.Fecha)
            .ToList();

        var totalGeneral = porDia.Sum(d => d.Total);
        var pedidosCount = pedidos.Count;

        var porMetodo = pagos
            .GroupBy(p => p.Metodo.ToString())
            .Select(g => new { Metodo = g.Key, Total = g.Sum(x => x.Monto) })
            .OrderByDescending(x => x.Total)
            .ToList();

        var top5 = pedidos.SelectMany(p => p.Detalles)
            .GroupBy(d => d.Producto.Nombre)
            .Select(g => new { Nombre = g.Key, Cant = g.Sum(d => d.Cantidad), Total = g.Sum(d => d.Subtotal) })
            .OrderByDescending(x => x.Total)
            .Take(5)
            .ToList();

        var periodo = $"{desde:dd/MM/yyyy} - {hasta:dd/MM/yyyy}";

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontFamily("Courier New").FontSize(9).FontColor("#f0f0f0"));
                page.Background().Background(Colors.Grey.Darken4);

                page.Header().Column(col =>
                {
                    col.Item().Text("La Mesa del Duque").FontSize(20).Bold().FontColor("#d4af37");
                    col.Item().Text("Reporte de Ventas - Resumen Gerencial").FontSize(12).FontColor("#d4af37");
                    col.Item().Text($"Periodo: {periodo}").FontSize(10).FontColor("#cccccc");
                    col.Item().PaddingTop(4).LineHorizontal(1).LineColor("#d4af37");
                });

                page.Content().PaddingTop(12).Column(col =>
                {
                    col.Item().Text("Ventas por Dia").FontSize(11).Bold().FontColor("#d4af37");
                    col.Item().PaddingTop(4).Table(tbl =>
                    {
                        tbl.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                        });

                        tbl.Header(header =>
                        {
                            header.Cell().Background("#d4af37").Padding(4).Text("Fecha").Bold().FontColor(Colors.Grey.Darken4).FontSize(9);
                            header.Cell().Background("#d4af37").Padding(4).Text("Pedidos").Bold().FontColor(Colors.Grey.Darken4).FontSize(9);
                            header.Cell().Background("#d4af37").Padding(4).Text("Total").Bold().FontColor(Colors.Grey.Darken4).FontSize(9);
                        });

                        bool alt1 = false;
                        foreach (var d in porDia)
                        {
                            var bg = alt1 ? "#2a2a3e" : "#1a1a2e";
                            tbl.Cell().Background(bg).Padding(3).Text(d.Fecha.ToString("dd/MM/yyyy")).FontColor("#f0f0f0").FontSize(8);
                            tbl.Cell().Background(bg).Padding(3).AlignRight().Text(d.Count.ToString()).FontColor("#f0f0f0").FontSize(8);
                            tbl.Cell().Background(bg).Padding(3).AlignRight().Text($"${d.Total:N2}").FontColor("#f0f0f0").FontSize(8);
                            alt1 = !alt1;
                        }
                        tbl.Cell().Background("#d4af37").Padding(3).Text("TOTAL").Bold().FontColor(Colors.Grey.Darken4).FontSize(8);
                        tbl.Cell().Background("#d4af37").Padding(3).AlignRight().Text(pedidosCount.ToString()).Bold().FontColor(Colors.Grey.Darken4).FontSize(8);
                        tbl.Cell().Background("#d4af37").Padding(3).AlignRight().Text($"${totalGeneral:N2}").Bold().FontColor(Colors.Grey.Darken4).FontSize(8);
                    });

                    col.Item().PaddingTop(14).Text("Totales por Metodo de Pago").FontSize(11).Bold().FontColor("#d4af37");
                    col.Item().PaddingTop(4).Table(tbl =>
                    {
                        tbl.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3);
                            c.RelativeColumn(2);
                        });

                        tbl.Header(header =>
                        {
                            header.Cell().Background("#d4af37").Padding(4).Text("Metodo").Bold().FontColor(Colors.Grey.Darken4).FontSize(9);
                            header.Cell().Background("#d4af37").Padding(4).Text("Total").Bold().FontColor(Colors.Grey.Darken4).FontSize(9);
                        });

                        bool alt2 = false;
                        foreach (var m in porMetodo)
                        {
                            var bg = alt2 ? "#2a2a3e" : "#1a1a2e";
                            tbl.Cell().Background(bg).Padding(3).Text(m.Metodo).FontColor("#f0f0f0").FontSize(8);
                            tbl.Cell().Background(bg).Padding(3).AlignRight().Text($"${m.Total:N2}").FontColor("#f0f0f0").FontSize(8);
                            alt2 = !alt2;
                        }
                    });

                    col.Item().PaddingTop(14).Text("Top 5 Productos").FontSize(11).Bold().FontColor("#d4af37");
                    col.Item().PaddingTop(4).Table(tbl =>
                    {
                        tbl.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(4);
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                        });

                        tbl.Header(header =>
                        {
                            header.Cell().Background("#d4af37").Padding(4).Text("Producto").Bold().FontColor(Colors.Grey.Darken4).FontSize(9);
                            header.Cell().Background("#d4af37").Padding(4).Text("Cant.").Bold().FontColor(Colors.Grey.Darken4).FontSize(9);
                            header.Cell().Background("#d4af37").Padding(4).Text("Total").Bold().FontColor(Colors.Grey.Darken4).FontSize(9);
                        });

                        bool alt3 = false;
                        foreach (var p in top5)
                        {
                            var bg = alt3 ? "#2a2a3e" : "#1a1a2e";
                            tbl.Cell().Background(bg).Padding(3).Text(p.Nombre).FontColor("#f0f0f0").FontSize(8);
                            tbl.Cell().Background(bg).Padding(3).AlignRight().Text(p.Cant.ToString()).FontColor("#f0f0f0").FontSize(8);
                            tbl.Cell().Background(bg).Padding(3).AlignRight().Text($"${p.Total:N2}").FontColor("#f0f0f0").FontSize(8);
                            alt3 = !alt3;
                        }
                    });
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}   |   Pagina ").FontColor("#888888").FontSize(8);
                    t.CurrentPageNumber().FontColor("#888888").FontSize(8);
                    t.Span(" de ").FontColor("#888888").FontSize(8);
                    t.TotalPages().FontColor("#888888").FontSize(8);
                });
            });
        });

        return doc.GeneratePdf();
    }

    // ── Kardex Excel ──────────────────────────────────────────────────────────

    public async Task<byte[]> GenerarKardexExcelAsync(
        DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        var ingredientes = await _uot.Ingredientes.ObtenerTodosAsync(ct);
        var mermas = await _uot.Mermas.ObtenerPorRangoAsync(desde, hasta, ct);

        var mermasPorIng = mermas
            .GroupBy(m => m.IngredienteId)
            .ToDictionary(
                g => g.Key,
                g => new { TotalMerma = g.Sum(m => m.CantidadDescartada), CostoMerma = g.Sum(m => m.CostoEstimado) });

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Kardex Inventario");

        var hds = new[] { "Ingrediente", "Unidad", "Stock Actual", "Total Mermas", "Costo Mermas", "Costo Unitario", "Valor Stock" };
        for (int i = 0; i < hds.Length; i++)
        {
            var c = ws.Cell(1, i + 1);
            c.Value = hds[i];
            c.Style.Font.Bold = true;
            c.Style.Fill.BackgroundColor = XLColor.FromHtml("#1a1a2e");
            c.Style.Font.FontColor = XLColor.FromHtml("#d4af37");
        }

        int row = 2;
        foreach (var ing in ingredientes.OrderBy(i => i.Nombre))
        {
            var m = mermasPorIng.TryGetValue(ing.Id, out var v) ? v : null;
            ws.Cell(row, 1).Value = ing.Nombre;
            ws.Cell(row, 2).Value = ing.UnidadMedida;
            ws.Cell(row, 3).Value = ing.StockActual;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.##";
            ws.Cell(row, 4).Value = m?.TotalMerma ?? 0;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.##";
            ws.Cell(row, 5).Value = m?.CostoMerma ?? 0;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 6).Value = ing.CostoUnitario;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 7).Value = ing.StockActual * ing.CostoUnitario;
            ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
            row++;
        }

        ws.Cell(row, 1).Value = "TOTAL";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 5).Value = mermas.Sum(m => m.CostoEstimado);
        ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 5).Style.Font.Bold = true;
        ws.Cell(row, 7).Value = ingredientes.Sum(i => i.StockActual * i.CostoUnitario);
        ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 7).Style.Font.Bold = true;

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ── Mermas PDF ────────────────────────────────────────────────────────────

    public async Task<byte[]> GenerarReporteMermasPdfAsync(
        DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        var mermas = await _uot.Mermas.ObtenerPorRangoAsync(desde, hasta, ct);
        var totalCosto = mermas.Sum(m => m.CostoEstimado);
        var periodo = $"{desde:dd/MM/yyyy} - {hasta:dd/MM/yyyy}";
        var lista = mermas.OrderBy(m => m.CreatedAt).ToList();
        var listaCount = lista.Count;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontFamily("Courier New").FontSize(9).FontColor("#f0f0f0"));
                page.Background().Background(Colors.Grey.Darken4);

                page.Header().Column(col =>
                {
                    col.Item().Text("La Mesa del Duque").FontSize(20).Bold().FontColor("#d4af37");
                    col.Item().Text("Reporte de Mermas").FontSize(12).FontColor("#d4af37");
                    col.Item().Text($"Periodo: {periodo}").FontSize(10).FontColor("#cccccc");
                    col.Item().PaddingTop(4).LineHorizontal(1).LineColor("#d4af37");
                });

                page.Content().PaddingTop(12).Column(col =>
                {
                    col.Item().Text($"Total registros: {listaCount}   |   Costo total: ${totalCosto:N2}")
                        .FontSize(10).Bold().FontColor("#d4af37");

                    col.Item().PaddingTop(8).Table(tbl =>
                    {
                        tbl.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn(3);
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                        });

                        tbl.Header(header =>
                        {
                            header.Cell().Background("#d4af37").Padding(3).Text("Fecha").Bold().FontColor(Colors.Grey.Darken4).FontSize(8);
                            header.Cell().Background("#d4af37").Padding(3).Text("Ingrediente").Bold().FontColor(Colors.Grey.Darken4).FontSize(8);
                            header.Cell().Background("#d4af37").Padding(3).Text("Cant.").Bold().FontColor(Colors.Grey.Darken4).FontSize(8);
                            header.Cell().Background("#d4af37").Padding(3).Text("Tipo").Bold().FontColor(Colors.Grey.Darken4).FontSize(8);
                            header.Cell().Background("#d4af37").Padding(3).Text("Costo").Bold().FontColor(Colors.Grey.Darken4).FontSize(8);
                            header.Cell().Background("#d4af37").Padding(3).Text("Usuario").Bold().FontColor(Colors.Grey.Darken4).FontSize(8);
                            header.Cell().Background("#d4af37").Padding(3).Text("Motivo").Bold().FontColor(Colors.Grey.Darken4).FontSize(8);
                        });

                        bool alt = false;
                        foreach (var m in lista)
                        {
                            var bg = alt ? "#2a2a3e" : "#1a1a2e";
                            tbl.Cell().Background(bg).Padding(3).Text(m.CreatedAt.ToLocalTime().ToString("dd/MM/yy HH:mm")).FontColor("#f0f0f0").FontSize(7);
                            tbl.Cell().Background(bg).Padding(3).Text(m.Ingrediente?.Nombre ?? "-").FontColor("#f0f0f0").FontSize(7);
                            tbl.Cell().Background(bg).Padding(3).AlignRight().Text($"{m.CantidadDescartada:N2}").FontColor("#f0f0f0").FontSize(7);
                            tbl.Cell().Background(bg).Padding(3).Text(m.Tipo.ToString()).FontColor("#f0f0f0").FontSize(7);
                            tbl.Cell().Background(bg).Padding(3).AlignRight().Text($"${m.CostoEstimado:N2}").FontColor("#f0f0f0").FontSize(7);
                            tbl.Cell().Background(bg).Padding(3).Text(m.Usuario?.NombreCompleto ?? "-").FontColor("#f0f0f0").FontSize(7);
                            tbl.Cell().Background(bg).Padding(3).Text(m.Notas ?? "-").FontColor("#888888").FontSize(7);
                            alt = !alt;
                        }

                        tbl.Cell().ColumnSpan(4).Background("#d4af37").Padding(3)
                            .Text("COSTO TOTAL").Bold().FontColor(Colors.Grey.Darken4).FontSize(8);
                        tbl.Cell().Background("#d4af37").Padding(3).AlignRight()
                            .Text($"${totalCosto:N2}").Bold().FontColor(Colors.Grey.Darken4).FontSize(8);
                        tbl.Cell().ColumnSpan(2).Background("#d4af37").Padding(3).Text("").FontSize(8);
                    });
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}   |   Pagina ").FontColor("#888888").FontSize(8);
                    t.CurrentPageNumber().FontColor("#888888").FontSize(8);
                    t.Span(" de ").FontColor("#888888").FontSize(8);
                    t.TotalPages().FontColor("#888888").FontSize(8);
                });
            });
        });

        return doc.GeneratePdf();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<List<Pedido>> ObtenerPedidosPagadosAsync(
        DateTime desde, DateTime hasta, CancellationToken ct)
    {
        var todos = await _uot.Pedidos.ObtenerTodosAsync(ct);
        return todos
            .Where(p => p.Estado == EstadoPedido.Pagado
                        && p.FechaCreacion >= desde
                        && p.FechaCreacion <= hasta)
            .ToList();
    }
}
