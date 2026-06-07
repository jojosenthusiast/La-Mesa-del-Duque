using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Web.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using MeseroIndexModel = LaMesaDelDuque.Web.Pages.Operaciones.Mesero.IndexModel;

namespace LaMesaDelDuque.Pruebas.Web;

public class MeseroPageTests
{
    [Fact]
    public async Task OnPostPagarJsonAsync_Tarjeta_DebeEnviarMetodoYReferenciaAlServicio()
    {
        var pedidos = new FakeMeseroPedidosServicio();
        var page = CreatePage(pedidos);

        var result = await page.OnPostPagarJsonAsync(pedidos.Pedido.Id, "tarjeta", pedidos.Pedido.Total, " AUTH-123 ");

        Assert.IsType<JsonResult>(result);
        Assert.Equal(1, pedidos.PagarCalls);
        Assert.Equal(pedidos.Pedido.Id, pedidos.LastPedidoId);
        Assert.Equal(MetodoPago.Tarjeta, pedidos.LastMetodoPago);
        Assert.Equal("AUTH-123", pedidos.LastReferenciaPos);
    }

    [Fact]
    public async Task OnPostPagarJsonAsync_Qr_DebeEnviarTransferenciaYReferenciaAlServicio()
    {
        var pedidos = new FakeMeseroPedidosServicio();
        var page = CreatePage(pedidos);

        var result = await page.OnPostPagarJsonAsync(pedidos.Pedido.Id, "qr", pedidos.Pedido.Total, " QR-987 ");

        Assert.IsType<JsonResult>(result);
        Assert.Equal(1, pedidos.PagarCalls);
        Assert.Equal(MetodoPago.Transferencia, pedidos.LastMetodoPago);
        Assert.Equal("QR-987", pedidos.LastReferenciaPos);
    }

    [Fact]
    public async Task OnPostPagarJsonAsync_TarjetaSinReferencia_DebeRechazarSinLlamarServicio()
    {
        var pedidos = new FakeMeseroPedidosServicio();
        var page = CreatePage(pedidos);

        var result = await page.OnPostPagarJsonAsync(pedidos.Pedido.Id, "tarjeta", pedidos.Pedido.Total, "   ");

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(0, pedidos.PagarCalls);
    }

    [Fact]
    public async Task OnPostPagarJsonAsync_EfectivoInsuficiente_DebeRechazarSinLlamarServicio()
    {
        var pedidos = new FakeMeseroPedidosServicio();
        var page = CreatePage(pedidos);

        var result = await page.OnPostPagarJsonAsync(pedidos.Pedido.Id, "efectivo", pedidos.Pedido.Total - 1m);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(0, pedidos.PagarCalls);
    }

    [Fact]
    public async Task OnGetAsync_marca_productos_con_receta_para_confirmacion()
    {
        var pedidos = new FakeMeseroPedidosServicio();
        var catalogo = new FakeMeseroCatalogoServicio();
        var page = CreatePage(pedidos, catalogo, new FakeMeseroRecetasServicio(catalogo.ProductoActivoId));

        await page.OnGetAsync();

        Assert.Contains(catalogo.ProductoActivoId, page.ProductosConReceta);
    }

    [Fact]
    public async Task OnPostCrearConItemsJsonAsync_reenvia_notas_y_modificaciones_al_servicio()
    {
        var pedidos = new FakeMeseroPedidosServicio();
        var catalogo = new FakeMeseroCatalogoServicio();
        var page = CreatePage(pedidos, catalogo);
        const string modificacionesJson = "[{\"accion\":\"confirmado\"}]";
        var itemsJson = $"[{{\"productoId\":\"{catalogo.ProductoActivoId}\",\"cantidad\":1,\"notas\":\"Sin sal\",\"modificacionesJson\":{System.Text.Json.JsonSerializer.Serialize(modificacionesJson)}}}]";

        await page.OnPostCrearConItemsJsonAsync(Guid.NewGuid(), itemsJson);

        var detalle = Assert.Single(pedidos.DetallesCreados);
        Assert.Equal("Sin sal", detalle.Notas);
        Assert.Equal(modificacionesJson, detalle.ModificacionesJson);
    }

    private static MeseroIndexModel CreatePage(
        FakeMeseroPedidosServicio pedidos,
        FakeMeseroCatalogoServicio? catalogo = null,
        FakeMeseroRecetasServicio? recetas = null) =>
        new(pedidos,
            catalogo ?? new FakeMeseroCatalogoServicio(),
            new FakeMeseroMesasServicio(),
            recetas ?? new FakeMeseroRecetasServicio(),
            new FakeMeseroHubContext<PedidosHub>(),
            NullLogger<MeseroIndexModel>.Instance);

    private sealed class FakeMeseroPedidosServicio : IPedidosServicio
    {
        public PedidoDto Pedido { get; } = new()
        {
            Id = Guid.NewGuid(),
            Estado = "Abierto",
            MesaNumero = 7,
            TipoServicio = "ComerAqui",
            Total = 42.50m,
            Detalles = [new DetallePedidoDto { Id = Guid.NewGuid(), ProductoId = Guid.NewGuid(), ProductoNombre = "Tosta", Cantidad = 1, PrecioUnitario = 42.50m, Subtotal = 42.50m }]
        };

        public int PagarCalls { get; private set; }
        public Guid? LastPedidoId { get; private set; }
        public MetodoPago? LastMetodoPago { get; private set; }
        public string? LastReferenciaPos { get; private set; }
        public List<DetalleCreacionDto> DetallesCreados { get; private set; } = [];

        public Task PagarPedidoAsync(Guid pedidoId, MetodoPago metodoPago = MetodoPago.Efectivo, string? referenciaPos = null, CancellationToken cancelacion = default)
        {
            PagarCalls++;
            LastPedidoId = pedidoId;
            LastMetodoPago = metodoPago;
            LastReferenciaPos = referenciaPos;
            return Task.CompletedTask;
        }

        public Task<PedidoDto?> ObtenerPedidoAsync(Guid pedidoId, CancellationToken cancelacion = default) =>
            Task.FromResult<PedidoDto?>(pedidoId == Pedido.Id ? Pedido : null);

        public Task<PedidoDto> CrearPedidoAsync(TipoServicio tipoServicio, Guid? mesaId, List<DetalleCreacionDto> detalles, CancellationToken cancelacion = default)
        {
            DetallesCreados = detalles;
            return Task.FromResult(Pedido);
        }

        public Task<PedidoDto> AgregarDetalleAsync(Guid pedidoId, Guid productoId, int cantidad, decimal precioUnitario, string? notas = null, string? modificacionesJson = null, CancellationToken cancelacion = default) => Task.FromResult(Pedido);
        public Task AgregarItemsAsync(Guid pedidoId, List<DetalleCreacionDto> items, CancellationToken cancelacion = default) => Task.CompletedTask;
        public Task<PedidoDto> EliminarDetalleAsync(Guid pedidoId, Guid detalleId, CancellationToken cancelacion = default) => Task.FromResult(Pedido);
        public Task<PedidoDto> ActualizarCantidadDetalleAsync(Guid pedidoId, Guid detalleId, int nuevaCantidad, CancellationToken cancelacion = default) => Task.FromResult(Pedido);
        public Task MarcarEnPreparacionAsync(Guid pedidoId, CancellationToken cancelacion = default) => Task.CompletedTask;
        public Task MarcarListoAsync(Guid pedidoId, CancellationToken cancelacion = default) => Task.CompletedTask;
        public Task CancelarPedidoAsync(Guid pedidoId, CancellationToken cancelacion = default) => Task.CompletedTask;
        public Task AnularPagoAsync(Guid pedidoId, CancellationToken cancelacion = default) => Task.CompletedTask;
        public Task EliminarPedidoPendienteAsync(Guid pedidoId, Guid usuarioId, string? ipAddress = null, CancellationToken cancelacion = default) => Task.CompletedTask;
        public Task<List<PedidoDto>> ListarPedidosActivosAsync(CancellationToken cancelacion = default) => Task.FromResult(new List<PedidoDto> { Pedido });
        public Task<List<PedidoDto>> ListarListosParaDespachoAsync(CancellationToken cancelacion = default) => Task.FromResult(new List<PedidoDto>());
        public Task MarcarEnCobroAsync(Guid pedidoId, CancellationToken cancelacion = default) => Task.CompletedTask;
        public Task<List<CuentaDto>> CrearCuentasAsync(Guid pedidoId, int cantidad, CancellationToken cancelacion = default) => Task.FromResult(new List<CuentaDto>());
        public Task<List<CuentaDto>> CrearCuentasConItemsAsync(Guid pedidoId, Dictionary<int, List<(Guid detalleId, int cantidad)>> asignaciones, CancellationToken cancelacion = default) => Task.FromResult(new List<CuentaDto>());
        public Task<CuentaDto> PagarCuentaAsync(Guid cuentaId, MetodoPago metodoPago, decimal propinaMonto = 0, string? referenciaPos = null, CancellationToken cancelacion = default) => Task.FromResult(new CuentaDto());
        public Task<List<CuentaDto>> ObtenerCuentasAsync(Guid pedidoId, CancellationToken cancelacion = default) => Task.FromResult(new List<CuentaDto>());
    }

    private sealed class FakeMeseroCatalogoServicio : ICatalogoProductosServicio
    {
        public Guid ProductoActivoId { get; } = Guid.NewGuid();
        public Guid ProductoInactivoId { get; } = Guid.NewGuid();

        public Task<List<CategoriaProductoDto>> ListarCategoriasAsync(CancellationToken cancelacion = default) => Task.FromResult(new List<CategoriaProductoDto>());
        public Task<CategoriaProductoDto> CrearCategoriaAsync(string nombre, CancellationToken cancelacion = default) => throw new NotImplementedException();
        public Task<CategoriaProductoDto> ActualizarCategoriaAsync(Guid categoriaId, string nombre, CancellationToken cancelacion = default) => throw new NotImplementedException();
        public Task DesactivarCategoriaAsync(Guid categoriaId, CancellationToken cancelacion = default) => throw new NotImplementedException();
        public Task<List<ProductoDto>> ListarProductosAsync(CancellationToken cancelacion = default) => Task.FromResult(new List<ProductoDto>
        {
            new() { Id = ProductoActivoId, Nombre = "Tosta", CategoriaNombre = "Entradas", CategoriaId = Guid.NewGuid(), Precio = 42.50m, Activo = true },
            new() { Id = ProductoInactivoId, Nombre = "Inactivo", CategoriaNombre = "Entradas", CategoriaId = Guid.NewGuid(), Precio = 1m, Activo = false }
        });
        public Task<List<ProductoDto>> ListarProductosPorCategoriaAsync(Guid categoriaId, CancellationToken cancelacion = default) => Task.FromResult(new List<ProductoDto>());
        public Task<ProductoDto> CrearProductoAsync(string nombre, decimal precio, Guid categoriaId, string? descripcion = null, string? imagenUrl = null, int tiempoPreparacionMin = 5, CancellationToken cancelacion = default) => throw new NotImplementedException();
        public Task<ProductoDto> ActualizarProductoAsync(Guid productoId, string nombre, decimal precio, Guid categoriaId, string? descripcion, string? imagenUrl = null, int? tiempoPreparacionMin = null, CancellationToken cancelacion = default) => throw new NotImplementedException();
        public Task DesactivarProductoAsync(Guid productoId, CancellationToken cancelacion = default) => throw new NotImplementedException();
    }

    private sealed class FakeMeseroRecetasServicio : IRecetasProductosServicio
    {
        private readonly Guid? _productoConRecetaId;

        public FakeMeseroRecetasServicio(Guid? productoConRecetaId = null)
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

    private sealed class FakeMeseroMesasServicio : IMesasServicio
    {
        public Task<List<MesaDto>> ListarMesasAsync(CancellationToken cancelacion = default) => Task.FromResult(new List<MesaDto>());
        public Task<MesaDto?> ObtenerMesaPorNumeroAsync(int numero, CancellationToken cancelacion = default) => Task.FromResult<MesaDto?>(null);
        public Task<MesaDto> CrearMesaAsync(int numero, int capacidad, CancellationToken cancelacion = default) => throw new NotImplementedException();
        public Task<MesaDto> ActualizarMesaAsync(Guid mesaId, int numero, int capacidad, CancellationToken cancelacion = default) => throw new NotImplementedException();
        public Task CambiarEstadoMesaAsync(Guid mesaId, string nuevoEstado, CancellationToken cancelacion = default) => throw new NotImplementedException();
        public Task DesactivarMesaAsync(Guid mesaId, CancellationToken cancelacion = default) => throw new NotImplementedException();
        public Task<MesaDto> ActualizarPosicionAsync(Guid mesaId, int posicionX, int posicionY, Guid zonaId, string forma, int? rotacion = null, CancellationToken cancelacion = default) => throw new NotImplementedException();
        public Task<MesaDto> LimpiarPosicionAsync(Guid mesaId, CancellationToken cancelacion = default) => throw new NotImplementedException();
    }

    private sealed class FakeMeseroHubContext<THub> : IHubContext<THub> where THub : Hub
    {
        public IHubClients Clients => new FakeMeseroHubClients();
        public IGroupManager Groups => new FakeMeseroGroupManager();
    }

    private sealed class FakeMeseroHubClients : IHubClients
    {
        public IClientProxy All => new FakeMeseroClientProxy();
        public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => new FakeMeseroClientProxy();
        public IClientProxy Client(string connectionId) => new FakeMeseroClientProxy();
        public IClientProxy Clients(IReadOnlyList<string> connectionIds) => new FakeMeseroClientProxy();
        public IClientProxy Group(string groupName) => new FakeMeseroClientProxy();
        public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => new FakeMeseroClientProxy();
        public IClientProxy Groups(IReadOnlyList<string> groupNames) => new FakeMeseroClientProxy();
        public IClientProxy User(string userId) => new FakeMeseroClientProxy();
        public IClientProxy Users(IReadOnlyList<string> userIds) => new FakeMeseroClientProxy();
    }

    private sealed class FakeMeseroClientProxy : IClientProxy
    {
        public Task SendCoreAsync(string method, object?[]? args, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeMeseroGroupManager : IGroupManager
    {
        public Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
