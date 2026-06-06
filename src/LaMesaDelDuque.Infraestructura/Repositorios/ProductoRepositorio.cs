using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class ProductoRepositorio : IProductoRepositorio
{
    private readonly LaMesaDelDuqueDbContext _contexto;

    public ProductoRepositorio(LaMesaDelDuqueDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<Producto?> ObtenerPorIdAsync(Guid id, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Producto>()
            .AsNoTracking()
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id, cancelacion);
    }

    public async Task<Producto?> ObtenerConTrackingAsync(Guid id, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Producto>()
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id, cancelacion);
    }

    public async Task<List<Producto>> ObtenerTodosAsync(CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Producto>()
            .AsNoTracking()
            .Include(p => p.Categoria)
            .ToListAsync(cancelacion);
    }

    public async Task<List<Producto>> ObtenerPorCategoriaAsync(Guid categoriaId, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Producto>()
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Where(p => EF.Property<Guid>(p, "CategoriaId") == categoriaId)
            .ToListAsync(cancelacion);
    }

    public async Task AgregarAsync(Producto producto, CancellationToken cancelacion = default)
    {
        await _contexto.Set<Producto>().AddAsync(producto, cancelacion);
    }

    public async Task<bool> ExisteEnPedidosActivosAsync(Guid productoId, CancellationToken cancelacion = default)
    {
        return await _contexto.Set<Pedido>()
            .Where(p => p.Estado == EstadoPedido.Pendiente || p.Estado == EstadoPedido.EnPreparacion)
            .AnyAsync(p => p.Detalles.Any(d => d.Producto.Id == productoId), cancelacion);
    }
}
