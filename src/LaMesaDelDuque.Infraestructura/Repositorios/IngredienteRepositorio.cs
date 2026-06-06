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

    public async Task<Ingrediente?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default) =>
        await _contexto.Set<Ingrediente>().FirstOrDefaultAsync(x => x.Id == id, cancelacion);

    public async Task<Ingrediente?> ObtenerPorIdConProveedorAsync(Guid id, CancellationToken cancelacion = default) =>
        await _contexto.Set<Ingrediente>()
            .Include(i => i.ProveedorDefault)
            .FirstOrDefaultAsync(x => x.Id == id, cancelacion);

    public async Task<List<Ingrediente>> ObtenerTodosAsync(CancellationToken cancelacion = default) =>
        await _contexto.Set<Ingrediente>().AsNoTracking().ToListAsync(cancelacion);

    public async Task<List<Ingrediente>> ObtenerTodosConProveedorAsync(CancellationToken cancelacion = default) =>
        await _contexto.Set<Ingrediente>()
            .Include(i => i.ProveedorDefault)
            .AsNoTracking()
            .OrderBy(i => i.Nombre)
            .ToListAsync(cancelacion);

    public async Task<List<Ingrediente>> ObtenerPorProveedorIdAsync(Guid proveedorId, CancellationToken cancelacion = default) =>
        await _contexto.Set<Ingrediente>()
            .AsNoTracking()
            .Where(i => EF.Property<Guid?>(i, "ProveedorDefaultId") == proveedorId)
            .OrderBy(i => i.Nombre)
            .ToListAsync(cancelacion);

    public async Task AgregarAsync(Ingrediente ingrediente, CancellationToken cancelacion = default) =>
        await _contexto.Set<Ingrediente>().AddAsync(ingrediente, cancelacion);
}
