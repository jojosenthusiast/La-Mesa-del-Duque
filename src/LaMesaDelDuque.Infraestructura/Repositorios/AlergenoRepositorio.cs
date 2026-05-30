using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class AlergenoRepositorio : IAlergenoRepositorio
{
    private readonly LaMesaDelDuqueDbContext _contexto;

    public AlergenoRepositorio(LaMesaDelDuqueDbContext contexto) => _contexto = contexto;

    public async Task<List<Alergeno>> ObtenerTodosAsync(CancellationToken cancelacion = default) =>
        await _contexto.Set<Alergeno>().AsNoTracking().ToListAsync(cancelacion);

    public async Task<List<Alergeno>> ObtenerActivosAsync(CancellationToken cancelacion = default) =>
        await _contexto.Set<Alergeno>().Where(a => a.Activo).AsNoTracking().ToListAsync(cancelacion);

    public async Task<Alergeno?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default) =>
        await _contexto.Set<Alergeno>().FirstOrDefaultAsync(a => a.Id == id, cancelacion);

    public async Task<List<ProductoAlergeno>> ObtenerPorProductoAsync(Guid productoId, CancellationToken cancelacion = default) =>
        await _contexto.Set<ProductoAlergeno>().Include(pa => pa.Alergeno).Where(pa => pa.ProductoId == productoId).AsNoTracking().ToListAsync(cancelacion);

    public async Task AgregarProductoAlergenoAsync(ProductoAlergeno pa, CancellationToken cancelacion = default) =>
        await _contexto.Set<ProductoAlergeno>().AddAsync(pa, cancelacion);

    public async Task EliminarProductoAlergenoAsync(Guid productoAlergenoId, CancellationToken cancelacion = default)
    {
        var pa = await _contexto.Set<ProductoAlergeno>().FindAsync([productoAlergenoId], cancelacion);
        if (pa != null) _contexto.Set<ProductoAlergeno>().Remove(pa);
    }
}
