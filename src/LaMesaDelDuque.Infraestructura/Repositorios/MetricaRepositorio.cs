using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Modelos;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class MetricaRepositorio : IMetricaRepositorio
{
    private readonly LaMesaDelDuqueDbContext _contexto;

    public MetricaRepositorio(LaMesaDelDuqueDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<MetricasOperativasDto> ObtenerMetricasHoyAsync(DateTime inicioTurno, CancellationToken ct = default)
    {
        var pedidosHoyQuery = _contexto.Set<Pedido>()
            .AsNoTracking()
            .Where(p => p.FechaCreacion >= inicioTurno && p.Estado != EstadoPedido.Cancelado);

        var detallesHoy = await pedidosHoyQuery
            .SelectMany(p => p.Detalles)
            .Select(d => d.Cantidad * d.PrecioUnitario)
            .ToListAsync(ct);

        var ventasHoy = detallesHoy.Sum();

        var pedidosHoyCount = await pedidosHoyQuery
            .CountAsync(ct);

        var totalMesas = await _contexto.Set<Mesa>()
            .AsNoTracking()
            .CountAsync(ct);

        var mesasActivas = await pedidosHoyQuery
            .Where(p => p.Mesa != null && p.Estado != EstadoPedido.Pagado)
            .Select(p => p.Mesa!.Id)
            .Distinct()
            .CountAsync(ct);

        var pedidosExcedenSLA = await _contexto.Set<OrdenCocina>()
            .AsNoTracking()
            .Where(o => o.Estado == EstadoLineaCocina.EnPreparacion
                     && o.HoraRecibido < DateTime.UtcNow.AddMinutes(-30))
            .Select(o => o.PedidoId)
            .Distinct()
            .CountAsync(ct);

        var turnoverRate = totalMesas > 0
            ? (decimal)pedidosHoyCount / totalMesas
            : 0m;

        return new MetricasOperativasDto
        {
            VentasHoy = ventasHoy,
            MesasActivas = mesasActivas,
            TotalMesas = totalMesas,
            TurnoverRate = turnoverRate,
            PedidosExcedenSLA = pedidosExcedenSLA
        };
    }

    public async Task<List<VentaPorHoraDto>> ObtenerVentasPorHoraAsync(DateTime inicioTurno, CancellationToken ct = default)
    {
        var pedidosHoy = await _contexto.Set<Pedido>()
            .AsNoTracking()
            .Include(p => p.Detalles)
            .Where(p => p.FechaCreacion >= inicioTurno && p.Estado != EstadoPedido.Cancelado)
            .ToListAsync(ct);

        var ventasPorHora = pedidosHoy
            .Select(p => new { Hora = p.FechaCreacion.Hour, Total = p.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario) })
            .GroupBy(x => x.Hora)
            .Select(g => new VentaPorHoraDto
            {
                Hora = g.Key,
                Total = g.Sum(x => x.Total)
            })
            .OrderBy(v => v.Hora)
            .ToList();

        // Rellenar horas faltantes con 0
        var todasLasHoras = Enumerable.Range(0, 24)
            .Select(h => new VentaPorHoraDto { Hora = h, Total = ventasPorHora.FirstOrDefault(v => v.Hora == h)?.Total ?? 0m })
            .ToList();

        return todasLasHoras;
    }
}
