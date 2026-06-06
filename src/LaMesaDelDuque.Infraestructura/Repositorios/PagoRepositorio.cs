using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class PagoRepositorio : IPagoRepositorio
{
    private readonly LaMesaDelDuqueDbContext _contexto;

    public PagoRepositorio(LaMesaDelDuqueDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<Pago?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default) =>
        await _contexto.Set<Pago>().AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancelacion);

    public async Task<List<Pago>> ObtenerPorCuentaAsync(Guid cuentaId, CancellationToken cancelacion = default) =>
        await _contexto.Set<Pago>().AsNoTracking().Where(p => p.CuentaId == cuentaId).ToListAsync(cancelacion);

    public async Task<List<Pago>> ObtenerDelDiaAsync(DateOnly fecha, CancellationToken cancelacion = default)
    {
        var inicio = fecha.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var fin = inicio.AddDays(1);
        return await _contexto.Set<Pago>()
            .AsNoTracking()
            .Where(p => p.FechaPago >= inicio && p.FechaPago < fin)
            .ToListAsync(cancelacion);
    }

    public async Task<List<Pago>> ObtenerPorRangoFechaAsync(DateTime desde, DateTime hasta, CancellationToken cancelacion = default) =>
        await _contexto.Set<Pago>()
            .AsNoTracking()
            .Where(p => p.FechaPago >= desde && p.FechaPago <= hasta)
            .ToListAsync(cancelacion);

    public async Task AgregarAsync(Pago pago, CancellationToken cancelacion = default) =>
        await _contexto.Set<Pago>().AddAsync(pago, cancelacion);
}
