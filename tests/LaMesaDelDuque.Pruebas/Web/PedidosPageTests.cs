using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Web.Hubs;
using LaMesaDelDuque.Web.Pages.Operaciones.Pedidos;
using Microsoft.AspNetCore.SignalR;

namespace LaMesaDelDuque.Pruebas.Web;

public class PedidosPageTests
{
    [Fact]
    public async Task OnGetAsync_carga_productos_mesas_y_pedidos_activos()
    {
        var catalogo = new FakeCatalogoPedidosProductosServicio();
        var mesas = new FakePedidosMesasServicio();
        var pedidos = new FakePedidosServicio();
        var cocina = new FakeCocinaServicio();
        var hub = new FakeHubContext<PedidosHub>();

        var page = new IndexModel(pedidos, catalogo, mesas, cocina, hub);

        await page.OnGetAsync();

        Assert.NotEmpty(page.Vm.ProductosDisponibles);
        Assert.All(page.Vm.ProductosDisponibles, p => Assert.True(p.Activo));
        Assert.NotEmpty(page.Vm.MesasDisponibles);
        Assert.NotEmpty(page.Vm.PedidosActivos);
    }

    [Fact]
    public async Task OnPostEnviarACocinaJsonAsync_genera_ordenes_y_devuelve_ok()
    {
        var pedidos = new FakePedidosServicio();
        var cocina = new FakeCocinaServicio();
        var hub = new FakeHubContext<PedidosHub>();
        var page = new IndexModel(pedidos, new FakeCatalogoPedidosProductosServicio(), new FakePedidosMesasServicio(), cocina, hub);

        var result = await page.OnPostEnviarACocinaJsonAsync(pedidos._pedido.Id);

        Assert.IsType<JsonResult>(result);
        var json = (JsonResult)result;
        Assert.True(json.Value?.GetType().GetProperty("ok")?.GetValue(json.Value) as bool?);
        Assert.True(cocina.WasCalled);
        Assert.True(hub.WasCalled);
    }
}

internal sealed class FakePedidosServicio : IPedidosServicio
{
    public readonly PedidoDto _pedido = new()
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

internal sealed class FakeCocinaServicio : ICocinaServicio
{
    public bool WasCalled { get; private set; }

    public Task GenerarOrdenesAsync(Guid pedidoId, CancellationToken ct = default)
    {
        WasCalled = true;
        return Task.CompletedTask;
    }

    public Task<List<OrdenCocinaDto>> ListarPendientesAsync(LaMesaDelDuque.Dominio.Enumeraciones.EstacionCocina? estacion = null, CancellationToken ct = default)
        => Task.FromResult(new List<OrdenCocinaDto>());

    public Task<OrdenCocinaDto> MarcarListoAsync(Guid ordenId, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<OrdenCocinaDto> RecuperarAsync(Guid ordenId, CancellationToken ct = default)
        => throw new NotImplementedException();
}

internal sealed class FakeHubContext<THub> : IHubContext<THub> where THub : Hub
{
    public bool WasCalled { get; private set; }

    public IHubClients Clients => new FakeHubClients(() => WasCalled = true);
    public IGroupManager Groups => new FakeGroupManager();
}

internal sealed class FakeHubClients : IHubClients
{
    private readonly Action _onSend;

    public FakeHubClients(Action onSend) { _onSend = onSend; }

    public IClientProxy All => new FakeClientProxy(_onSend);
    public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => new FakeClientProxy(_onSend);
    public IClientProxy Client(string connectionId) => new FakeClientProxy(_onSend);
    public IClientProxy Clients(IReadOnlyList<string> connectionIds) => new FakeClientProxy(_onSend);
    public IClientProxy Group(string groupName) => new FakeClientProxy(_onSend);
    public IClientProxy Groups(IReadOnlyList<string> groupNames) => new FakeClientProxy(_onSend);
    public IClientProxy User(string userId) => new FakeClientProxy(_onSend);
    public IClientProxy Users(IReadOnlyList<string> userIds) => new FakeClientProxy(_onSend);
}

internal sealed class FakeClientProxy : IClientProxy
{
    private readonly Action _onSend;
    public FakeClientProxy(Action onSend) { _onSend = onSend; }

    public Task SendCoreAsync(string method, object?[]? args, CancellationToken cancellationToken = default)
    {
        _onSend();
        return Task.CompletedTask;
    }
}

internal sealed class FakeGroupManager : IGroupManager
{
    public Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
