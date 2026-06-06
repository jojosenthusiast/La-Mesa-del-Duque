using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class CuentaRepositorio : ICuentaRepositorio
{
    private readonly LaMesaDelDuqueDbContext _contexto;

    public CuentaRepositorio(LaMesaDelDuqueDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<Cuenta?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Cuenta>()
            .AsNoTracking()
            .Include(c => c.DetallesAsignados)
            .FirstOrDefaultAsync(c => c.Id == id, cancelacion);
    }

    public async Task<Cuenta?> ObtenerParaActualizarAsync(Guid id, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Cuenta>()
            .Include(c => c.DetallesAsignados)
            .FirstOrDefaultAsync(c => c.Id == id, cancelacion);
    }

    public async Task<List<Cuenta>> ObtenerPorPedidoAsync(Guid pedidoId, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Cuenta>()
            .AsNoTracking()
            .Include(c => c.DetallesAsignados)
            .Where(c => c.PedidoId == pedidoId)
            .ToListAsync(cancelacion);
    }

    public async Task AgregarAsync(Cuenta cuenta, CancellationToken cancelacion = default)
    {
        await _contexto.Set<Cuenta>().AddAsync(cuenta, cancelacion);
    }

    public void Eliminar(Cuenta cuenta)
    {
        _contexto.Set<Cuenta>().Remove(cuenta);
    }
}
