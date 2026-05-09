using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Web.Pages.Operaciones.Pedidos;

namespace LaMesaDelDuque.Pruebas.Web;

public class PedidosPageTests
{
    [Fact]
    public async Task OnGetAsync_carga_productos_mesas_y_pedidos_activos()
    {
        var catalogo = new FakeCatalogoPedidosProductosServicio();
        var mesas = new FakePedidosMesasServicio();
        var pedidos = new FakePedidosServicio();

        var page = new IndexModel(pedidos, catalogo, mesas);

        await page.OnGetAsync();

        Assert.NotEmpty(page.Vm.ProductosDisponibles);
        Assert.All(page.Vm.ProductosDisponibles, p => Assert.True(p.Activo));
        Assert.NotEmpty(page.Vm.MesasDisponibles);
        Assert.NotEmpty(page.Vm.PedidosActivos);
    }
}

internal sealed class FakePedidosServicio : IPedidosServicio
{
    private readonly PedidoDto _pedido = new()
    {
        Id = Guid.NewGuid(),
        Estado = "Abierto",
        MesaNumero = 1,
        TipoServicio = "ComerAqui",
        Total = 20,
        Detalles = [new DetallePedidoDto { Id = Guid.NewGuid(), ProductoId = Guid.NewGuid(), ProductoNombre = "Sopa", Cantidad = 1, PrecioUnitario = 20, Subtotal = 20 }]
    };

    public Task<PedidoDto> CrearPedidoAsync(LaMesaDelDuque.Dominio.Enumeraciones.TipoServicio tipoServicio, Guid? mesaId, List<DetalleCreacionDto> detalles, CancellationToken cancelacion = default) => Task.FromResult(_pedido);
    public Task<PedidoDto> AgregarDetalleAsync(Guid pedidoId, Guid productoId, int cantidad, decimal precioUnitario, CancellationToken cancelacion = default) => Task.FromResult(_pedido);
    public Task<PedidoDto> EliminarDetalleAsync(Guid pedidoId, Guid detalleId, CancellationToken cancelacion = default) => Task.FromResult(_pedido);
    public Task<PedidoDto> ActualizarCantidadDetalleAsync(Guid pedidoId, Guid detalleId, int nuevaCantidad, CancellationToken cancelacion = default) => Task.FromResult(_pedido);
    public Task MarcarEnPreparacionAsync(Guid pedidoId, CancellationToken cancelacion = default) => Task.CompletedTask;
    public Task PagarPedidoAsync(Guid pedidoId, CancellationToken cancelacion = default) => Task.CompletedTask;
    public Task CancelarPedidoAsync(Guid pedidoId, CancellationToken cancelacion = default) => Task.CompletedTask;
    public Task EliminarPedidoPendienteAsync(Guid pedidoId, Guid usuarioId, string? ipAddress = null, CancellationToken cancelacion = default) => Task.CompletedTask;
    public Task<PedidoDto?> ObtenerPedidoAsync(Guid pedidoId, CancellationToken cancelacion = default) => Task.FromResult<PedidoDto?>(_pedido);
    public Task<List<PedidoDto>> ListarPedidosActivosAsync(CancellationToken cancelacion = default) => Task.FromResult(new List<PedidoDto> { _pedido });
}

internal sealed class FakeCatalogoPedidosProductosServicio : ICatalogoProductosServicio
{
    public Task<List<CategoriaProductoDto>> ListarCategoriasAsync(CancellationToken cancelacion = default) => Task.FromResult(new List<CategoriaProductoDto>());
    public Task<CategoriaProductoDto> CrearCategoriaAsync(string nombre, CancellationToken cancelacion = default) => throw new NotImplementedException();
    public Task<CategoriaProductoDto> ActualizarCategoriaAsync(Guid categoriaId, string nombre, CancellationToken cancelacion = default) => throw new NotImplementedException();
    public Task DesactivarCategoriaAsync(Guid categoriaId, CancellationToken cancelacion = default) => throw new NotImplementedException();
    public Task<List<ProductoDto>> ListarProductosAsync(CancellationToken cancelacion = default)
        => Task.FromResult(new List<ProductoDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Sopa", CategoriaNombre = "Entradas", CategoriaId = Guid.NewGuid(), Precio = 20, Activo = true },
            new() { Id = Guid.NewGuid(), Nombre = "Inactivo", CategoriaNombre = "Entradas", CategoriaId = Guid.NewGuid(), Precio = 10, Activo = false }
        });
    public Task<List<ProductoDto>> ListarProductosPorCategoriaAsync(Guid categoriaId, CancellationToken cancelacion = default) => Task.FromResult(new List<ProductoDto>());
    public Task<ProductoDto> CrearProductoAsync(string nombre, decimal precio, Guid categoriaId, string? descripcion = null, string? imagenUrl = null, int tiempoPreparacionMin = 5, CancellationToken cancelacion = default) => throw new NotImplementedException();
    public Task<ProductoDto> ActualizarProductoAsync(Guid productoId, string nombre, decimal precio, Guid categoriaId, string? descripcion, string? imagenUrl = null, int? tiempoPreparacionMin = null, CancellationToken cancelacion = default) => throw new NotImplementedException();
    public Task DesactivarProductoAsync(Guid productoId, CancellationToken cancelacion = default) => throw new NotImplementedException();
}

internal sealed class FakePedidosMesasServicio : IMesasServicio
{
    public Task<List<MesaDto>> ListarMesasAsync(CancellationToken cancelacion = default)
        => Task.FromResult(new List<MesaDto>
        {
            new() { Id = Guid.NewGuid(), Numero = 1, Capacidad = 4, Estado = "Disponible", Activa = true },
            new() { Id = Guid.NewGuid(), Numero = 9, Capacidad = 2, Estado = "Inactiva", Activa = false }
        });

    public Task<MesaDto?> ObtenerMesaPorNumeroAsync(int numero, CancellationToken cancelacion = default) => Task.FromResult<MesaDto?>(null);
    public Task<MesaDto> CrearMesaAsync(int numero, int capacidad, CancellationToken cancelacion = default) => throw new NotImplementedException();
    public Task<MesaDto> ActualizarMesaAsync(Guid mesaId, int numero, int capacidad, CancellationToken cancelacion = default) => throw new NotImplementedException();
    public Task CambiarEstadoMesaAsync(Guid mesaId, string nuevoEstado, CancellationToken cancelacion = default) => throw new NotImplementedException();
    public Task DesactivarMesaAsync(Guid mesaId, CancellationToken cancelacion = default) => throw new NotImplementedException();
}
