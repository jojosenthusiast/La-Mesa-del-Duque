using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class ZonaSalonRepositorio : IZonaSalonRepositorio
{
    private readonly LaMesaDelDuqueDbContext _contexto;

    public ZonaSalonRepositorio(LaMesaDelDuqueDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<ZonaSalon?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<ZonaSalon>()
            .AsNoTracking()
            .FirstOrDefaultAsync(z => z.Id == id, cancelacion);
    }

    public async Task<List<ZonaSalon>> ObtenerActivasOrdenadasAsync(CancellationToken cancelacion = default)
    {
        return await _contexto.Set<ZonaSalon>()
            .AsNoTracking()
            .Where(z => z.Activa)
            .OrderBy(z => z.Orden)
            .ThenBy(z => z.Nombre)
            .ToListAsync(cancelacion);
    }

    public async Task<List<ZonaSalon>> ObtenerTodasAsync(CancellationToken cancelacion = default)
    {
        return await _contexto.Set<ZonaSalon>()
            .AsNoTracking()
            .OrderBy(z => z.Orden)
            .ThenBy(z => z.Nombre)
            .ToListAsync(cancelacion);
    }

    public async Task AgregarAsync(ZonaSalon zona, CancellationToken cancelacion = default)
    {
        await _contexto.Set<ZonaSalon>().AddAsync(zona, cancelacion);
    }

    public async Task<ZonaSalon?> ObtenerParaActualizarAsync(Guid id, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<ZonaSalon>()
            .FirstOrDefaultAsync(z => z.Id == id, cancelacion);
    }
}
