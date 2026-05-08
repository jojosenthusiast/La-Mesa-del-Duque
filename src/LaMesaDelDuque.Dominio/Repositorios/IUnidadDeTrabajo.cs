namespace LaMesaDelDuque.Dominio.Repositorios;

public interface IUnidadDeTrabajo
{
    ICategoriaProductoRepositorio Categorias { get; }
    IProductoRepositorio Productos { get; }
    IMesaRepositorio Mesas { get; }
    IPedidoRepositorio Pedidos { get; }
    IUsuarioRepositorio Usuarios { get; }
    IAuditoriaRepositorio Auditorias { get; }

    Task<int> GuardarCambiosAsync(CancellationToken cancelacion = default);
}
