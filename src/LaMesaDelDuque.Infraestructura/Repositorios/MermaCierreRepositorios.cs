using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class MermaRepositorio : IMermaRepositorio
{
    private readonly LaMesaDelDuqueDbContext _c;
    public MermaRepositorio(LaMesaDelDuqueDbContext c) => _c = c;

    public async Task AgregarAsync(MermaDiaria m, CancellationToken ct = default) =>
        await _c.Set<MermaDiaria>().AddAsync(m, ct);

    public async Task<List<MermaDiaria>> ObtenerDelDiaAsync(DateOnly fecha, CancellationToken ct = default) =>
        await _c.Set<MermaDiaria>().Include(m => m.Ingrediente).Where(m => m.CreatedAt.Date == fecha.ToDateTime(TimeOnly.MinValue).Date).AsNoTracking().ToListAsync(ct);
}

internal class CierreDiaRepositorio : ICierreDiaRepositorio
{
    private readonly LaMesaDelDuqueDbContext _c;
    public CierreDiaRepositorio(LaMesaDelDuqueDbContext c) => _c = c;

    public async Task<CierreDia?> ObtenerAbiertoAsync(DateOnly fecha, CancellationToken ct = default)
    {
        var date = fecha.ToDateTime(TimeOnly.MinValue);
        return await _c.Set<CierreDia>().FirstOrDefaultAsync(c => c.Fecha == fecha, ct);
    }

    public async Task AgregarAsync(CierreDia cierre, CancellationToken ct = default) =>
        await _c.Set<CierreDia>().AddAsync(cierre, ct);

    public async Task<List<CierreDia>> ObtenerTodosAsync(CancellationToken ct = default) =>
        await _c.Set<CierreDia>().OrderByDescending(c => c.Fecha).Take(30).AsNoTracking().ToListAsync(ct);
}
