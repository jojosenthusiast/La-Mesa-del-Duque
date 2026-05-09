using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class RolRepositorio : IRolRepositorio
{
    private readonly LaMesaDelDuqueDbContext _contexto;

    public RolRepositorio(LaMesaDelDuqueDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<Rol?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Rol>().FirstOrDefaultAsync(r => r.Id == id, cancelacion);
    }

    public async Task<Rol?> ObtenerPorNombreAsync(string nombre, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Rol>().FirstOrDefaultAsync(r => r.Nombre == nombre, cancelacion);
    }

    public async Task<List<Rol>> ObtenerTodosAsync(CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Rol>().AsNoTracking().ToListAsync(cancelacion);
    }

    public async Task AgregarAsync(Rol rol, CancellationToken cancelacion = default)
    {
        await _contexto.Set<Rol>().AddAsync(rol, cancelacion);
    }
}
