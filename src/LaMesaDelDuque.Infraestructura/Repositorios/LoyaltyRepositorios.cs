using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class ClienteLoyaltyRepositorio : IClienteRepositorio
{
    private readonly LaMesaDelDuqueDbContext _c;
    public ClienteLoyaltyRepositorio(LaMesaDelDuqueDbContext c) => _c = c;

    public async Task<Cliente?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        await _c.Set<Cliente>().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<List<Cliente>> BuscarAsync(string q, CancellationToken ct = default) =>
        await _c.Set<Cliente>().Where(c => c.Nombre.Contains(q) || c.Telefono.Contains(q)).Take(20).AsNoTracking().ToListAsync(ct);

    public async Task AgregarAsync(Cliente c, CancellationToken ct = default) =>
        await _c.Set<Cliente>().AddAsync(c, ct);
}

internal class RecompensaLoyaltyRepositorio : IRecompensaRepositorio
{
    private readonly LaMesaDelDuqueDbContext _c;
    public RecompensaLoyaltyRepositorio(LaMesaDelDuqueDbContext c) => _c = c;

    public async Task<Recompensa?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        await _c.Set<Recompensa>().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<List<Recompensa>> ObtenerActivasAsync(CancellationToken ct = default) =>
        await _c.Set<Recompensa>().Where(r => r.Activo).AsNoTracking().ToListAsync(ct);
}
