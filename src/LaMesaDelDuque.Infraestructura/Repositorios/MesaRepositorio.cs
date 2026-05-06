using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class MesaRepositorio : IMesaRepositorio
{
    private readonly LaMesaDelDuqueDbContext _contexto;

    public MesaRepositorio(LaMesaDelDuqueDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<Mesa?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Mesa>()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, cancelacion);
    }

    public async Task<Mesa?> ObtenerPorNumeroAsync(int numero, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Mesa>()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Numero == numero, cancelacion);
    }

    public async Task<List<Mesa>> ObtenerTodasAsync(CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Mesa>()
            .AsNoTracking()
            .OrderBy(m => m.Numero)
            .ToListAsync(cancelacion);
    }

    public async Task AgregarAsync(Mesa mesa, CancellationToken cancelacion = default)
    {
        await _contexto.Set<Mesa>().AddAsync(mesa, cancelacion);
    }

    public async Task<Mesa?> ObtenerParaActualizarAsync(Guid id, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Mesa>()
            .FirstOrDefaultAsync(m => m.Id == id, cancelacion);
    }
}
