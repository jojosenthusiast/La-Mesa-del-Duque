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
        PedidoRepositorio pedidoRepositorio,
        UsuarioRepositorio usuarioRepositorio,
        AuditoriaRepositorio auditoriaRepositorio)
    {
        _contexto = contexto;
        Categorias = categoriaRepositorio;
        Productos = productoRepositorio;
        Mesas = mesaRepositorio;
        Pedidos = pedidoRepositorio;
        Usuarios = usuarioRepositorio;
        Auditorias = auditoriaRepositorio;
    }

    public ICategoriaProductoRepositorio Categorias { get; }
    public IProductoRepositorio Productos { get; }
    public IMesaRepositorio Mesas { get; }
    public IPedidoRepositorio Pedidos { get; }
    public IUsuarioRepositorio Usuarios { get; }
    public IAuditoriaRepositorio Auditorias { get; }

    public async Task<int> GuardarCambiosAsync(CancellationToken cancelacion = default)
    {
        return await _contexto.SaveChangesAsync(cancelacion);
    }
}
