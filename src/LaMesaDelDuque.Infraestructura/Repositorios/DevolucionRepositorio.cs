using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class DevolucionRepositorio : IDevolucionRepositorio
{
    private readonly LaMesaDelDuqueDbContext _contexto;

    public DevolucionRepositorio(LaMesaDelDuqueDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<List<DevolucionPago>> ObtenerPorPagoAsync(Guid pagoOriginalId, CancellationToken cancelacion = default) =>
        await _contexto.Set<DevolucionPago>()
            .Include(d => d.PagoOriginal)
            .AsNoTracking()
            .Where(d => d.PagoOriginalId == pagoOriginalId)
            .OrderByDescending(d => d.FechaHora)
            .ToListAsync(cancelacion);

    public async Task<List<DevolucionPago>> ObtenerPorFechaAsync(DateOnly fecha, CancellationToken cancelacion = default)
    {
        var inicio = fecha.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var fin = inicio.AddDays(1);
        return await _contexto.Set<DevolucionPago>()
            .Include(d => d.PagoOriginal)
            .AsNoTracking()
            .Where(d => d.FechaHora >= inicio && d.FechaHora < fin)
            .OrderByDescending(d => d.FechaHora)
            .ToListAsync(cancelacion);
    }

    public async Task<List<DevolucionPago>> ObtenerPorRangoFechaAsync(DateTime desde, DateTime hasta, CancellationToken cancelacion = default) =>
        await _contexto.Set<DevolucionPago>()
            .Include(d => d.PagoOriginal)
            .AsNoTracking()
            .Where(d => d.FechaHora >= desde && d.FechaHora <= hasta)
            .OrderByDescending(d => d.FechaHora)
            .ToListAsync(cancelacion);

    public async Task AgregarAsync(DevolucionPago devolucion, CancellationToken cancelacion = default) =>
        await _contexto.Set<DevolucionPago>().AddAsync(devolucion, cancelacion);
}
