using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class OrdenCocinaRepositorio : IOrdenCocinaRepositorio
{
    private readonly LaMesaDelDuqueDbContext _contexto;

    public OrdenCocinaRepositorio(LaMesaDelDuqueDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<OrdenCocina?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<OrdenCocina>()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancelacion);
    }

    public async Task<List<OrdenCocina>> ListarPendientesAsync(EstacionCocina? estacion = null, CancellationToken cancelacion = default)
    {
        var consulta = _contexto.Set<OrdenCocina>()
            .AsNoTracking()
            .Where(o => o.Estado != EstadoLineaCocina.Listo)
            .OrderBy(o => o.HoraRecibido);

        if (estacion.HasValue)
        {
            consulta = consulta.Where(o => o.Estacion == estacion.Value)
                .OrderBy(o => o.HoraRecibido);
        }

        return await consulta.ToListAsync(cancelacion);
    }

    public async Task AgregarAsync(OrdenCocina orden, CancellationToken cancelacion = default)
    {
        await _contexto.Set<OrdenCocina>().AddAsync(orden, cancelacion);
    }

    public void Eliminar(OrdenCocina orden)
    {
        _contexto.Set<OrdenCocina>().Remove(orden);
    }
}
