using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class CategoriaProductoRepositorio : ICategoriaProductoRepositorio
{
    private readonly LaMesaDelDuqueDbContext _contexto;

    public CategoriaProductoRepositorio(LaMesaDelDuqueDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<CategoriaProducto?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<CategoriaProducto>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancelacion);
    }

    public async Task<CategoriaProducto?> ObtenerConTrackingAsync(Guid id, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<CategoriaProducto>()
            .FirstOrDefaultAsync(c => c.Id == id, cancelacion);
    }

    public async Task<List<CategoriaProducto>> ObtenerTodasAsync(CancellationToken cancelacion = default)
    {
        return await _contexto.Set<CategoriaProducto>()
            .AsNoTracking()
            .ToListAsync(cancelacion);
    }

    public async Task AgregarAsync(CategoriaProducto categoria, CancellationToken cancelacion = default)
    {
        await _contexto.Set<CategoriaProducto>().AddAsync(categoria, cancelacion);
    }
}
