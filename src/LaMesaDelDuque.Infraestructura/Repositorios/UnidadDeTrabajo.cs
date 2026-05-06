using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;

namespace LaMesaDelDuque.Infraestructura.Repositorios;

internal class UnidadDeTrabajo : IUnidadDeTrabajo
{
    private readonly LaMesaDelDuqueDbContext _contexto;

    public UnidadDeTrabajo(
        LaMesaDelDuqueDbContext contexto,
        CategoriaProductoRepositorio categoriaRepositorio,
        ProductoRepositorio productoRepositorio,
        MesaRepositorio mesaRepositorio,
        PedidoRepositorio pedidoRepositorio)
    {
        _contexto = contexto;
        Categorias = categoriaRepositorio;
        Productos = productoRepositorio;
        Mesas = mesaRepositorio;
        Pedidos = pedidoRepositorio;
    }

    public ICategoriaProductoRepositorio Categorias { get; }
    public IProductoRepositorio Productos { get; }
    public IMesaRepositorio Mesas { get; }
    public IPedidoRepositorio Pedidos { get; }

    public async Task<int> GuardarCambiosAsync(CancellationToken cancelacion = default)
    {
        return await _contexto.SaveChangesAsync(cancelacion);
    }
}
