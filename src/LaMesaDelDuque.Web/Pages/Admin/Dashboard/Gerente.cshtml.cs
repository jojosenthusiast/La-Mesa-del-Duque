using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Modelos;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Web.Pages.Admin.Dashboard;

[Authorize(Roles = "Administrador,Gerente")]
public class GerenteModel : PageModel
{
    private readonly LaMesaDelDuqueDbContext _db;
    private readonly IMetricaServicio _metricas;
    private readonly ILogger<GerenteModel> _logger;

    public GerenteModel(LaMesaDelDuqueDbContext db, IMetricaServicio metricas, ILogger<GerenteModel> logger)
    {
        _db = db;
        _metricas = metricas;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Dashboard Gerencial";
        ViewData["ActiveTab"] = "Dashboard Gerencial";
    }

    public async Task<IActionResult> OnGetMetricasComparativasJsonAsync()
    {
        try
        {
            var ahora = DateTime.UtcNow;
            var inicio7d = ahora.AddDays(-7).Date;
            var inicio14d = ahora.AddDays(-14).Date;

            var pedidosPagados = await _db.Set<Pedido>()
                .Include(p => p.Detalles)
                .Where(p => p.Estado == EstadoPedido.Pagado && p.CreatedAt >= inicio14d)
                .ToListAsync();

            var pedidosPagadosDto = pedidosPagados.Select(p => new
            {
                p.CreatedAt,
                Total = p.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario)
            }).ToList();

            var semanaActual = pedidosPagadosDto
                .Where(p => p.CreatedAt >= inicio7d)
                .Sum(p => p.Total);

            var semanaAnterior = pedidosPagadosDto
                .Where(p => p.CreatedAt < inicio7d)
                .Sum(p => p.Total);

            decimal porcentajeCambio = semanaAnterior == 0
                ? 100
                : Math.Round((semanaActual - semanaAnterior) / semanaAnterior * 100, 1);

            return new JsonResult(new
            {
                semanaActual,
                semanaAnterior,
                porcentajeCambio,
                tendencia = porcentajeCambio >= 0 ? "subida" : "bajada"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en OnGetMetricasComparativasJsonAsync");
            return StatusCode(500, new { ok = false, error = "Ocurrió un error interno." });
        }
    }

    public async Task<IActionResult> OnGetTopProductosJsonAsync()
    {
        try
        {
            var hace30d = DateTime.UtcNow.AddDays(-30).Date;

            var pedidosIds = await _db.Set<Pedido>()
                .Where(p => p.Estado == EstadoPedido.Pagado && p.CreatedAt >= hace30d)
                .Select(p => p.Id)
                .ToListAsync();

            if (!pedidosIds.Any())
                return new JsonResult(Array.Empty<object>());

            var detalles = await _db.Set<DetallePedido>()
                .Include(d => d.Producto)
                .Where(d => pedidosIds.Contains(EF.Property<Guid>(d, "PedidoId")))
                .ToListAsync();

            var top = detalles
                .GroupBy(d => new { d.Producto.Id, d.Producto.Nombre })
                .Select(g => new
                {
                    productoId = g.Key.Id,
                    nombre = g.Key.Nombre,
                    cantidadTotal = g.Sum(d => d.Cantidad),
                    montoTotal = Math.Round(g.Sum(d => d.Cantidad * d.PrecioUnitario), 2)
                })
                .OrderByDescending(x => x.cantidadTotal)
                .Take(10)
                .ToList();

            return new JsonResult(top);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en OnGetTopProductosJsonAsync");
            return StatusCode(500, new { ok = false, error = "Ocurrió un error interno." });
        }
    }

    public async Task<IActionResult> OnGetTicketPromedioJsonAsync()
    {
        try
        {
            var hace30d = DateTime.UtcNow.AddDays(-30).Date;

            var pedidos = await _db.Set<Pedido>()
                .Include(p => p.Detalles)
                .Where(p => p.Estado == EstadoPedido.Pagado && p.CreatedAt >= hace30d)
                .ToListAsync();

            var pedidosTotales = pedidos.Select(p => new
            {
                Total = p.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario)
            }).ToList();

            var ticketPromedio = pedidosTotales.Count > 0
                ? Math.Round(pedidosTotales.Average(p => p.Total), 2)
                : 0;

            return new JsonResult(new
            {
                ticketPromedio,
                totalPedidos = pedidosTotales.Count,
                periodo = "últimos 30 días"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en OnGetTicketPromedioJsonAsync");
            return StatusCode(500, new { ok = false, error = "Ocurrió un error interno." });
        }
    }

    public async Task<IActionResult> OnGetVentasPorDiaSemanaJsonAsync()
    {
        try
        {
            var hace30d = DateTime.UtcNow.AddDays(-30).Date;

            var pedidosDia = await _db.Set<Pedido>()
                .Include(p => p.Detalles)
                .Where(p => p.Estado == EstadoPedido.Pagado && p.CreatedAt >= hace30d)
                .ToListAsync();

            var pedidos = pedidosDia.Select(p => new
            {
                DiaSemana = (int)p.CreatedAt.DayOfWeek,
                Total = p.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario)
            }).ToList();

            var diasNombres = new[] { "Dom", "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb" };

            var datos = Enumerable.Range(0, 7)
                .Select(i => new
                {
                    dia = diasNombres[i],
                    ventaTotal = Math.Round(pedidos.Where(p => p.DiaSemana == i).Sum(p => p.Total), 2),
                    pedidosCount = pedidos.Count(p => p.DiaSemana == i)
                })
                .ToList();

            return new JsonResult(datos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en OnGetVentasPorDiaSemanaJsonAsync");
            return StatusCode(500, new { ok = false, error = "Ocurrió un error interno." });
        }
    }
}
