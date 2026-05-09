using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class RecetaProductoRepositorio : IRecetaProductoRepositorio
{
    private readonly LaMesaDelDuqueDbContext _contexto;

    public RecetaProductoRepositorio(LaMesaDelDuqueDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<RecetaProducto?> ObtenerPorProductoIdAsync(Guid productoId, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<RecetaProducto>()
            .AsNoTracking()
            .Include(x => x.Producto)
            .Include(x => x.Ingredientes)
                .ThenInclude(x => x.Ingrediente)
            .FirstOrDefaultAsync(x => x.ProductoId == productoId, cancelacion);
    }

    public async Task AgregarAsync(RecetaProducto receta, CancellationToken cancelacion = default)
    {
        await _contexto.Set<RecetaProducto>().AddAsync(receta, cancelacion);
    }
}
