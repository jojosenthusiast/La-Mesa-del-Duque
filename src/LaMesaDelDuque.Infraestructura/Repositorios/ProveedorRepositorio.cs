using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class ProveedorRepositorio : IProveedorRepositorio
{
    private readonly LaMesaDelDuqueDbContext _c;
    public ProveedorRepositorio(LaMesaDelDuqueDbContext c) => _c = c;

    public async Task<Proveedor?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        await _c.Set<Proveedor>().FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<List<Proveedor>> ObtenerTodosAsync(CancellationToken ct = default) =>
        await _c.Set<Proveedor>().AsNoTracking().OrderBy(p => p.Nombre).ToListAsync(ct);

    public async Task AgregarAsync(Proveedor p, CancellationToken ct = default) =>
        await _c.Set<Proveedor>().AddAsync(p, ct);
}
