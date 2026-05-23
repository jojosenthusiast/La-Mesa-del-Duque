using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class IngredienteRepositorio : IIngredienteRepositorio
{
    private readonly LaMesaDelDuqueDbContext _contexto;

    public IngredienteRepositorio(LaMesaDelDuqueDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<Ingrediente?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Ingrediente>().FirstOrDefaultAsync(x => x.Id == id, cancelacion);
    }

    public async Task<List<Ingrediente>> ObtenerTodosAsync(CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Ingrediente>().Include(i => i.ProveedorDefault).AsNoTracking().ToListAsync(cancelacion);
    }
}
