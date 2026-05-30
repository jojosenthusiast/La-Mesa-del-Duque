using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class PromocionRepositorio : IPromocionRepositorio
{
    private readonly LaMesaDelDuqueDbContext _contexto;

    public PromocionRepositorio(LaMesaDelDuqueDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<List<PromocionProducto>> ObtenerActivasPorProductoAsync(Guid productoId, DateOnly fecha, CancellationToken ct = default)
    {
        return await _contexto.Set<PromocionProducto>()
            .AsNoTracking()
            .Include(pp => pp.Promocion)
            .Where(pp =>
                pp.ProductoId == productoId &&
                pp.Promocion.Activo &&
                pp.Promocion.FechaInicio <= fecha &&
                pp.Promocion.FechaFin >= fecha)
            .ToListAsync(ct);
    }
}
