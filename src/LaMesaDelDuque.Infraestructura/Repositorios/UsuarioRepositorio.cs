using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class UsuarioRepositorio : IUsuarioRepositorio
{
    private readonly LaMesaDelDuqueDbContext _contexto;

    public UsuarioRepositorio(LaMesaDelDuqueDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Usuario>()
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == id, cancelacion);
    }

    public async Task<Usuario?> ObtenerPorUsernameAsync(string username, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Usuario>()
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Username == username, cancelacion);
    }

    public async Task<List<Usuario>> ObtenerTodosAsync(CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Usuario>()
            .AsNoTracking()
            .Include(u => u.Rol)
            .ToListAsync(cancelacion);
    }

    public async Task AgregarAsync(Usuario usuario, CancellationToken cancelacion = default)
    {
        await _contexto.Set<Usuario>().AddAsync(usuario, cancelacion);
    }
}
