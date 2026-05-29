using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class PedidoRepositorio : IPedidoRepositorio
{
    private static readonly EstadoPedido[] EstadosMesaActiva =
    [
        EstadoPedido.Pendiente,
        EstadoPedido.EnPreparacion,
        EstadoPedido.EnCobro,
        EstadoPedido.Pagado,
        EstadoPedido.Listo
    ];

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

    // Registra explícitamente un DetallePedido nuevo como Added en el contexto.
    // Necesario porque DetallePedido.Id es un Guid asignado por el cliente (Guid.NewGuid()),
    // por lo que EF Core no puede inferir el estado Added por sí solo cuando descubre
    // la entidad durante DetectChanges (la trataría como Unchanged al tener PK no-cero).
    public Task AgregarDetalleAsync(DetallePedido detalle, CancellationToken cancelacion = default)
    {
        _contexto.Set<DetallePedido>().Add(detalle);
        return Task.CompletedTask;
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
            .Where(p => p.Mesa != null && p.Mesa.Id == mesaId && p.Estado != EstadoPedido.Cancelado && p.Estado != EstadoPedido.AnuladoPago)
            .ToListAsync(cancelacion);
    }


    public async Task<List<Pedido>> ObtenerActivosPorMeseroAsync(Guid meseroId, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Pedido>()
            .AsNoTracking()
            .Include(p => p.Mesa)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
            .Where(p => p.MeseroAsignadoId == meseroId
                     && p.Mesa != null
                     && EstadosMesaActiva.Contains(p.Estado))
            .OrderBy(p => p.Mesa!.Numero)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync(cancelacion);
    }

    public async Task<Pedido?> ObtenerActivoPorMesaParaActualizarAsync(Guid mesaId, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Pedido>()
            .Include(p => p.Mesa)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
            .Where(p => p.Mesa != null
                     && p.Mesa.Id == mesaId
                     && EstadosMesaActiva.Contains(p.Estado))
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancelacion);
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

    public async Task<int> ContarDelDiaAsync(DateOnly fecha, CancellationToken cancelacion = default)
    {
        var inicio = fecha.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var fin = inicio.AddDays(1);
        return await _contexto.Set<Pedido>()
            .Where(p => p.CreatedAt >= inicio && p.CreatedAt < fin)
            .CountAsync(cancelacion);
    }

    public async Task<int> ContarPagadosDelDiaAsync(DateOnly fecha, CancellationToken cancelacion = default)
    {
        var inicio = fecha.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var fin = inicio.AddDays(1);

        return await _contexto.Set<Pedido>()
            .Where(p => p.Estado != EstadoPedido.Cancelado
                     && p.Estado != EstadoPedido.AnuladoPago
                     && p.Cuentas.Any(c => _contexto.Set<Pago>()
                         .Any(pago => pago.CuentaId == c.Id
                                   && pago.FechaPago >= inicio
                                   && pago.FechaPago < fin)))
            .CountAsync(cancelacion);
    }
}
