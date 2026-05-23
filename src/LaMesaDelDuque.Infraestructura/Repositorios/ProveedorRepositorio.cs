using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class ProveedorLoyaltyRepositorio : IProveedorRepositorio
{
    private readonly LaMesaDelDuqueDbContext _c;
    public ProveedorLoyaltyRepositorio(LaMesaDelDuqueDbContext c) => _c = c;

    public async Task<List<Proveedor>> ObtenerTodosAsync(CancellationToken ct = default) =>
        await _c.Set<Proveedor>().AsNoTracking().ToListAsync(ct);

    public async Task AgregarAsync(Proveedor p, CancellationToken ct = default) =>
        await _c.Set<Proveedor>().AddAsync(p, ct);
}
