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
            .Where(p => p.FechaCreacion >= inicioTurno && p.Estado != EstadoPedido.Cancelado && p.Estado != EstadoPedido.AnuladoPago);

        var pagosHoy = await _contexto.Set<Pago>()
            .AsNoTracking()
            .Where(p => p.FechaPago >= inicioTurno)
            .Select(p => p.Monto)
            .ToListAsync(ct);

        var ventasHoy = pagosHoy.Sum();

        var pedidosHoyCount = await pedidosHoyQuery
            .CountAsync(ct);

        var totalMesas = await _contexto.Set<Mesa>()
            .AsNoTracking()
            .CountAsync(ct);

        var mesasActivas = await pedidosHoyQuery
            .Where(p => p.Mesa != null && p.Estado != EstadoPedido.Pagado && p.Estado != EstadoPedido.AnuladoPago)
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
        var pagosHoy = await _contexto.Set<Pago>()
            .AsNoTracking()
            .Where(p => p.FechaPago >= inicioTurno)
            .Select(p => new { Hora = p.FechaPago.Hour, Total = p.Monto })
            .ToListAsync(ct);

        var ventasPorHora = pagosHoy
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
