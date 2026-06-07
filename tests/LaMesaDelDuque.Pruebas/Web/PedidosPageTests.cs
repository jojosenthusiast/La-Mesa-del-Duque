using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Web.Hubs;
using LaMesaDelDuque.Web.Models.Operaciones;
using LaMesaDelDuque.Web.Pages.Operaciones.Pedidos;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;

namespace LaMesaDelDuque.Pruebas.Web;

public class PedidosPageTests
{
    [Fact]
    public async Task OnGetAsync_carga_productos_mesas_y_pedidos_activos()
    {
        var catalogo = new FakeCatalogoPedidosProductosServicio();
        var mesas = new FakePedidosMesasServicio();
        var pedidos = new FakePedidosServicio();
        var hub = new FakeHubContext<PedidosHub>();
        var recetas = new FakeRecetasProductosServicio();
        var ticket = new FakeTicketServicio();
        var alergenos = new FakeAlergenoServicio();

        var page = new IndexModel(pedidos, catalogo, mesas, recetas, ticket, alergenos, hub, NullLogger<IndexModel>.Instance);

        await page.OnGetAsync();

        Assert.NotEmpty(page.Vm.ProductosDisponibles);
        Assert.All(page.Vm.ProductosDisponibles, p => Assert.True(p.Activo));
        Assert.NotEmpty(page.Vm.MesasDisponibles);
        Assert.NotEmpty(page.Vm.PedidosActivos);
    }

    [Fact]
    public async Task OnGetAsync_marca_productos_con_receta_para_confirmacion()
    {
        var catalogo = new FakeCatalogoPedidosProductosServicio();
        var recetas = new FakeRecetasProductosServicio(catalogo.ProductoActivoId);
        var page = new IndexModel(
            new FakePedidosServicio(),
            catalogo,
            new FakePedidosMesasServicio(),
            recetas,
            new FakeTicketServicio(),
            new FakeAlergenoServicio(),
            new FakeHubContext<PedidosHub>(),
            NullLogger<IndexModel>.Instance);

        await page.OnGetAsync();

        Assert.Contains(catalogo.ProductoActivoId, page.ProductosConReceta);
        Assert.DoesNotContain(catalogo.ProductoInactivoId, page.ProductosConReceta);
    }

    [Fact]
    public async Task OnPostCrearAsync_reenvia_notas_y_modificaciones_al_servicio()
    {
        var catalogo = new FakeCatalogoPedidosProductosServicio();
        var pedidos = new FakePedidosServicio();
        var page = new IndexModel(
            pedidos,
            catalogo,
            new FakePedidosMesasServicio(),
            new FakeRecetasProductosServicio(),
            new FakeTicketServicio(),
            new FakeAlergenoServicio(),
            new FakeHubContext<PedidosHub>(),
            NullLogger<IndexModel>.Instance);
        const string modificacionesJson = "[{\"accion\":\"confirmado\"}]";
        page.Vm.CrearPedido.TipoServicio = "ParaLlevar";
        page.Vm.CrearPedido.Lineas.Add(new LineaPedidoFormVm
        {
            ProductoId = catalogo.ProductoActivoId,
            Cantidad = 2,
            Notas = "Sin cebolla",
            ModificacionesJson = modificacionesJson
        });

        await page.OnPostCrearAsync();

        var detalle = Assert.Single(pedidos.DetallesCreados);
        Assert.Equal("Sin cebolla", detalle.Notas);
        Assert.Equal(modificacionesJson, detalle.ModificacionesJson);
    }

    [Fact]
    public void PosJs_no_expone_intercambio_si_no_hay_catalogo_de_reemplazos_valido()
    {
        var repo = BuscarRaizRepositorio();
        var posJs = File.ReadAllText(Path.Combine(repo, "src", "LaMesaDelDuque.Web", "wwwroot", "js", "pos.js"));

        Assert.DoesNotContain("lmd-mod-ing-reemplazo", posJs);
        Assert.DoesNotContain("accion: 'intercambiar'", posJs);
        Assert.DoesNotContain("cambiarReemplazo", posJs);
    }

    public static string BuscarRaizRepositorio()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "LaMesaDelDuque.slnx")))
            dir = dir.Parent;

        return dir?.FullName ?? throw new InvalidOperationException("No se encontró LaMesaDelDuque.slnx.");
    }
}

public class TablesidePageTests
{
    [Fact]
    public async Task OnGetAsync_marca_productos_con_receta_para_confirmacion_tableside()
    {
        var catalogo = new FakeCatalogoPedidosProductosServicio();
        var page = new TablesideModel(
            new FakePedidosServicio(),
            catalogo,
            new FakePedidosMesasServicio(),
            new FakeRecetasProductosServicio(catalogo.ProductoActivoId),
            NullLogger<TablesideModel>.Instance);

        await page.OnGetAsync();

        Assert.Contains(catalogo.ProductoActivoId, page.ProductosConReceta);
    }

    [Fact]
    public async Task OnPostCrearJsonAsync_reenvia_notas_y_modificaciones_al_servicio_tableside()
    {
        var catalogo = new FakeCatalogoPedidosProductosServicio();
        var pedidos = new FakePedidosServicio();
        var page = new TablesideModel(
            pedidos,
            catalogo,
            new FakePedidosMesasServicio(),
            new FakeRecetasProductosServicio(catalogo.ProductoActivoId),
            NullLogger<TablesideModel>.Instance);
        const string modificacionesJson = "[{\"accion\":\"confirmado\"}]";
        page.Vm.CrearPedido.TipoServicio = "ParaLlevar";
        page.Vm.CrearPedido.Lineas.Add(new LineaPedidoFormVm
        {
            ProductoId = catalogo.ProductoActivoId,
            Cantidad = 1,
            Notas = "Confirmado en mesa",
            ModificacionesJson = modificacionesJson
        });

        await page.OnPostCrearJsonAsync();

        var detalle = Assert.Single(pedidos.DetallesCreados);
        Assert.Equal("Confirmado en mesa", detalle.Notas);
        Assert.Equal(modificacionesJson, detalle.ModificacionesJson);
    }

    [Fact]
    public void TablesideJs_envia_confirmacion_para_productos_con_receta()
    {
        var repo = PedidosPageTests.BuscarRaizRepositorio();
        var tablesideJs = File.ReadAllText(Path.Combine(repo, "src", "LaMesaDelDuque.Web", "wwwroot", "js", "tableside.js"));

        Assert.Contains("tieneReceta", tablesideJs);
        Assert.Contains("modificacionesJson", tablesideJs);
        Assert.Contains("Ingredientes confirmados", tablesideJs);
    }
}

internal sealed class FakeHubContext<THub> : IHubContext<THub> where THub : Hub
{
    public IHubClients Clients => new FakeHubClients();
    public IGroupManager Groups => new FakeGroupManager();
}

internal sealed class FakeHubClients : IHubClients
{
    public IClientProxy All => new FakeClientProxy();
    public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => new FakeClientProxy();
    public IClientProxy Client(string connectionId) => new FakeClientProxy();
    public IClientProxy Clients(IReadOnlyList<string> connectionIds) => new FakeClientProxy();
    public IClientProxy Group(string groupName) => new FakeClientProxy();
    public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => new FakeClientProxy();
    public IClientProxy Groups(IReadOnlyList<string> groupNames) => new FakeClientProxy();
    public IClientProxy User(string userId) => new FakeClientProxy();
    public IClientProxy Users(IReadOnlyList<string> userIds) => new FakeClientProxy();
}

internal sealed class FakeClientProxy : IClientProxy
{
    public Task SendCoreAsync(string method, object?[]? args, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal sealed class FakeGroupManager : IGroupManager
{
    public Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default) => Task.CompletedTask;
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
    public List<DetalleCreacionDto> DetallesCreados { get; private set; } = [];

    public Task<PedidoDto> CrearPedidoAsync(LaMesaDelDuque.Dominio.Enumeraciones.TipoServicio tipoServicio, Guid? mesaId, List<DetalleCreacionDto> detalles, CancellationToken cancelacion = default)
    {
        DetallesCreados = detalles;
        return Task.FromResult(_pedido);
    }
    public Task<PedidoDto> AgregarDetalleAsync(Guid pedidoId, Guid productoId, int cantidad, decimal precioUnitario, string? notas = null, string? modificacionesJson = null, CancellationToken cancelacion = default) => Task.FromResult(_pedido);
    public Task<PedidoDto> EliminarDetalleAsync(Guid pedidoId, Guid detalleId, CancellationToken cancelacion = default) => Task.FromResult(_pedido);
    public Task<PedidoDto> ActualizarCantidadDetalleAsync(Guid pedidoId, Guid detalleId, int nuevaCantidad, CancellationToken cancelacion = default) => Task.FromResult(_pedido);
    public Task MarcarEnPreparacionAsync(Guid pedidoId, CancellationToken cancelacion = default) => Task.CompletedTask;
    public Task PagarPedidoAsync(Guid pedidoId, LaMesaDelDuque.Dominio.Enumeraciones.MetodoPago metodoPago = LaMesaDelDuque.Dominio.Enumeraciones.MetodoPago.Efectivo, string? referenciaPos = null, CancellationToken cancelacion = default) => Task.CompletedTask;
    public Task CancelarPedidoAsync(Guid pedidoId, CancellationToken cancelacion = default) => Task.CompletedTask;
    public Task EliminarPedidoPendienteAsync(Guid pedidoId, Guid usuarioId, string? ipAddress = null, CancellationToken cancelacion = default) => Task.CompletedTask;
    public Task<PedidoDto?> ObtenerPedidoAsync(Guid pedidoId, CancellationToken cancelacion = default) => Task.FromResult<PedidoDto?>(_pedido);
    public Task<List<PedidoDto>> ListarPedidosActivosAsync(CancellationToken cancelacion = default) => Task.FromResult(new List<PedidoDto> { _pedido });
    public Task MarcarListoAsync(Guid pedidoId, CancellationToken cancelacion = default) => Task.CompletedTask;
    public Task<List<PedidoDto>> ListarListosParaDespachoAsync(CancellationToken cancelacion = default) => Task.FromResult(new List<PedidoDto>());
    public Task MarcarEnCobroAsync(Guid pedidoId, CancellationToken cancelacion = default) => Task.CompletedTask;
    public Task<List<CuentaDto>> CrearCuentasAsync(Guid pedidoId, int cantidad, CancellationToken cancelacion = default) => Task.FromResult(new List<CuentaDto>());
    public Task<List<CuentaDto>> CrearCuentasConItemsAsync(Guid pedidoId, Dictionary<int, List<(Guid detalleId, int cantidad)>> asignaciones, CancellationToken cancelacion = default) => Task.FromResult(new List<CuentaDto>());
    public Task<CuentaDto> PagarCuentaAsync(Guid cuentaId, LaMesaDelDuque.Dominio.Enumeraciones.MetodoPago metodoPago, decimal propinaMonto = 0, string? referenciaPos = null, CancellationToken cancelacion = default) => Task.FromResult(new CuentaDto());
    public Task<List<CuentaDto>> ObtenerCuentasAsync(Guid pedidoId, CancellationToken cancelacion = default) => Task.FromResult(new List<CuentaDto>());
    public Task AnularPagoAsync(Guid pedidoId, CancellationToken cancelacion = default) => Task.CompletedTask;
    public Task AgregarItemsAsync(Guid pedidoId, List<DetalleCreacionDto> items, CancellationToken cancelacion = default) => Task.CompletedTask;
}

internal sealed class FakeCatalogoPedidosProductosServicio : ICatalogoProductosServicio
{
    public Guid ProductoActivoId { get; } = Guid.NewGuid();
    public Guid ProductoInactivoId { get; } = Guid.NewGuid();

    public Task<List<CategoriaProductoDto>> ListarCategoriasAsync(CancellationToken cancelacion = default) => Task.FromResult(new List<CategoriaProductoDto>());
    public Task<CategoriaProductoDto> CrearCategoriaAsync(string nombre, CancellationToken cancelacion = default) => throw new NotImplementedException();
    public Task<CategoriaProductoDto> ActualizarCategoriaAsync(Guid categoriaId, string nombre, CancellationToken cancelacion = default) => throw new NotImplementedException();
    public Task DesactivarCategoriaAsync(Guid categoriaId, CancellationToken cancelacion = default) => throw new NotImplementedException();
    public Task<List<ProductoDto>> ListarProductosAsync(CancellationToken cancelacion = default)
        => Task.FromResult(new List<ProductoDto>
        {
            new() { Id = ProductoActivoId, Nombre = "Sopa", CategoriaNombre = "Entradas", CategoriaId = Guid.NewGuid(), Precio = 20, Activo = true },
            new() { Id = ProductoInactivoId, Nombre = "Inactivo", CategoriaNombre = "Entradas", CategoriaId = Guid.NewGuid(), Precio = 10, Activo = false }
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
    public Task<MesaDto> ActualizarPosicionAsync(Guid mesaId, int posicionX, int posicionY, Guid zonaId, string forma, int? rotacion = null, CancellationToken cancelacion = default) => throw new NotImplementedException();
    public Task<MesaDto> LimpiarPosicionAsync(Guid mesaId, CancellationToken cancelacion = default) => throw new NotImplementedException();
}

public class FakeRecetasProductosServicio : IRecetasProductosServicio
{
    private readonly Guid? _productoConRecetaId;

    public FakeRecetasProductosServicio(Guid? productoConRecetaId = null)
    {
        _productoConRecetaId = productoConRecetaId;
    }

    public Task<RecetaProductoDto> CrearRecetaAsync(Guid productoId, string instrucciones, List<RecetaIngredienteCreacionDto> ingredientes, CancellationToken cancelacion = default) => throw new NotImplementedException();
    public Task<RecetaProductoDto?> ObtenerPorProductoIdAsync(Guid productoId, CancellationToken cancelacion = default)
        => Task.FromResult(_productoConRecetaId == productoId
            ? new RecetaProductoDto
            {
                ProductoId = productoId,
                Ingredientes = [new RecetaIngredienteDto { IngredienteId = Guid.NewGuid(), IngredienteNombre = "Pan", CantidadRequerida = 1m }]
            }
            : null);
}

public class FakeTicketServicio : ITicketServicio
{
    public Task<string> GenerarHtmlTicketAsync(Guid pedidoId, CancellationToken cancelacion = default) => Task.FromResult("<html>Ticket</html>");
}

public class FakeAlergenoServicio : IAlergenoServicio
{
    public Task<List<AlergenoDto>> ObtenerActivosAsync(CancellationToken cancelacion = default) => Task.FromResult(new List<AlergenoDto>());
    public Task<List<AlergenoDto>> ObtenerPorProductoAsync(Guid productoId, CancellationToken cancelacion = default) => Task.FromResult(new List<AlergenoDto>());
}
