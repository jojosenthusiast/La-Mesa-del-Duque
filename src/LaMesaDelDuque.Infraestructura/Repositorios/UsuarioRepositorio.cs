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
}
