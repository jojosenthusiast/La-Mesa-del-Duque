using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class TurnoCajaRepositorio : ITurnoCajaRepositorio
{
    private readonly LaMesaDelDuqueDbContext _contexto;

    public TurnoCajaRepositorio(LaMesaDelDuqueDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<TurnoCaja?> ObtenerTurnoActivoAsync(CancellationToken ct = default)
    {
        return await _contexto.Set<TurnoCaja>()
            .AsNoTracking()
            .Include(t => t.Cajero)
            .Where(t => t.FechaCierre == null)
            .OrderByDescending(t => t.FechaApertura)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<TurnoCaja?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _contexto.Set<TurnoCaja>()
            .AsNoTracking()
            .Include(t => t.Cajero)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<List<TurnoCaja>> ObtenerHistorialAsync(int pagina, int porPagina, CancellationToken ct = default)
    {
        return await _contexto.Set<TurnoCaja>()
            .AsNoTracking()
            .Include(t => t.Cajero)
            .OrderByDescending(t => t.FechaApertura)
            .Skip((pagina - 1) * porPagina)
            .Take(porPagina)
            .ToListAsync(ct);
    }

    public async Task AgregarAsync(TurnoCaja turno, CancellationToken ct = default)
    {
        await _contexto.Set<TurnoCaja>().AddAsync(turno, ct);
    }
}
