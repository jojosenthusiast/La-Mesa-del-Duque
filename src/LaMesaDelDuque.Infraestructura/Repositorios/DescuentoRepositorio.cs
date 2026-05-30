using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class DescuentoRepositorio : IDescuentoRepositorio
{
    private readonly LaMesaDelDuqueDbContext _contexto;

    public DescuentoRepositorio(LaMesaDelDuqueDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<List<DescuentoAplicado>> ObtenerPorPedidoAsync(Guid pedidoId, CancellationToken cancelacion = default) =>
        await _contexto.Set<DescuentoAplicado>()
            .Include(d => d.Motivo)
            .AsNoTracking()
            .Where(d => d.PedidoId == pedidoId)
            .OrderByDescending(d => d.FechaSolicitud)
            .ToListAsync(cancelacion);

    public async Task<List<DescuentoAplicado>> ObtenerPendientesAsync(CancellationToken cancelacion = default) =>
        await _contexto.Set<DescuentoAplicado>()
            .Include(d => d.Motivo)
            .AsNoTracking()
            .Where(d => d.Estado == EstadoDescuento.Pendiente)
            .OrderBy(d => d.FechaSolicitud)
            .ToListAsync(cancelacion);

    public async Task<DescuentoAplicado?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default) =>
        await _contexto.Set<DescuentoAplicado>()
            .Include(d => d.Motivo)
            .FirstOrDefaultAsync(d => d.Id == id, cancelacion);

    public async Task<List<DescuentoAplicado>> ObtenerPorRangoFechaAsync(DateTime desde, DateTime hasta, CancellationToken cancelacion = default) =>
        await _contexto.Set<DescuentoAplicado>()
            .Include(d => d.Motivo)
            .AsNoTracking()
            .Where(d => d.FechaSolicitud >= desde && d.FechaSolicitud <= hasta)
            .OrderByDescending(d => d.FechaSolicitud)
            .ToListAsync(cancelacion);

    public async Task AgregarAsync(DescuentoAplicado descuento, CancellationToken cancelacion = default) =>
        await _contexto.Set<DescuentoAplicado>().AddAsync(descuento, cancelacion);
}
