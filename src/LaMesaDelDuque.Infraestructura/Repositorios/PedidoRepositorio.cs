using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class PedidoRepositorio : IPedidoRepositorio
{
    private readonly LaMesaDelDuqueDbContext _contexto;

    public PedidoRepositorio(LaMesaDelDuqueDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<Pedido?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Pedido>()
            .AsNoTracking()
            .Include(p => p.Mesa)
            .FirstOrDefaultAsync(p => p.Id == id, cancelacion);
    }

    public async Task<Pedido?> ObtenerConDetallesAsync(Guid id, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Pedido>()
            .AsNoTracking()
            .Include(p => p.Mesa)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
                    .ThenInclude(prod => prod.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id, cancelacion);
    }

    public async Task<Pedido?> ObtenerConDetallesParaActualizarAsync(Guid id, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Pedido>()
            .Include(p => p.Mesa)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
                    .ThenInclude(prod => prod.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id, cancelacion);
    }

    public async Task<List<Pedido>> ObtenerTodosAsync(CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Pedido>()
            .AsNoTracking()
            .Include(p => p.Mesa)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
            .ToListAsync(cancelacion);
    }

    public async Task AgregarAsync(Pedido pedido, CancellationToken cancelacion = default)
    {
        await _contexto.Set<Pedido>().AddAsync(pedido, cancelacion);
    }

    public void Eliminar(Pedido pedido)
    {
        _contexto.Set<Pedido>().Remove(pedido);
    }

    public async Task<List<Pedido>> ObtenerPorMesaAsync(Guid mesaId, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Pedido>()
            .AsNoTracking()
            .Include(p => p.Mesa)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
            .Where(p => p.Mesa != null && p.Mesa.Id == mesaId && p.Estado != EstadoPedido.Cancelado)
            .ToListAsync(cancelacion);
    }

    public async Task<Pedido?> ObtenerConCuentasParaActualizarAsync(Guid id, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Pedido>()
            .Include(p => p.Mesa)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
                    .ThenInclude(prod => prod.Categoria)
            .Include(p => p.Cuentas)
                .ThenInclude(c => c.DetallesAsignados)
            .FirstOrDefaultAsync(p => p.Id == id, cancelacion);
    }


    public async Task<int> ContarCanceladosDelDiaAsync(DateOnly fecha, CancellationToken cancelacion = default)
    {
        var inicio = fecha.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var fin = inicio.AddDays(1);
        return await _contexto.Set<Pedido>()
            .Where(p => p.Estado == EstadoPedido.Cancelado
                     && p.CreatedAt >= inicio && p.CreatedAt < fin)
            .CountAsync(cancelacion);
    }
}
