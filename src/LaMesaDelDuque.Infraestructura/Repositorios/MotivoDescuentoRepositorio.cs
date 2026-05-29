using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class MotivoDescuentoRepositorio : IMotivoDescuentoRepositorio
{
    private readonly LaMesaDelDuqueDbContext _contexto;

    public MotivoDescuentoRepositorio(LaMesaDelDuqueDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<List<MotivoDescuento>> ObtenerTodosActivosAsync(CancellationToken cancelacion = default) =>
        await _contexto.Set<MotivoDescuento>()
            .AsNoTracking()
            .Where(m => m.Activo)
            .OrderBy(m => m.Nombre)
            .ToListAsync(cancelacion);

    public async Task<List<MotivoDescuento>> ObtenerTodosAsync(CancellationToken cancelacion = default) =>
        await _contexto.Set<MotivoDescuento>()
            .AsNoTracking()
            .OrderBy(m => m.Nombre)
            .ToListAsync(cancelacion);

    public async Task<MotivoDescuento?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default) =>
        await _contexto.Set<MotivoDescuento>()
            .FirstOrDefaultAsync(m => m.Id == id, cancelacion);

    public async Task AgregarAsync(MotivoDescuento motivo, CancellationToken cancelacion = default) =>
        await _contexto.Set<MotivoDescuento>().AddAsync(motivo, cancelacion);

    public Task ActualizarAsync(MotivoDescuento motivo, CancellationToken cancelacion = default)
    {
        _contexto.Set<MotivoDescuento>().Update(motivo);
        return Task.CompletedTask;
    }
}
