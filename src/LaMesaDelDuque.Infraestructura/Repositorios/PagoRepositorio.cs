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

    public async Task<Pago?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Pago>().AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancelacion);
    }

    public async Task<List<Pago>> ObtenerPorCuentaAsync(Guid cuentaId, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Pago>().AsNoTracking().Where(p => p.CuentaId == cuentaId).ToListAsync(cancelacion);
    }

    public async Task AgregarAsync(Pago pago, CancellationToken cancelacion = default)
    {
        await _contexto.Set<Pago>().AddAsync(pago, cancelacion);
    }
}
