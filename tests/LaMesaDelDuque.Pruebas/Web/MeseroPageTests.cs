using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Pruebas.Calidad;
using LaMesaDelDuque.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using MeseroIndexModel = LaMesaDelDuque.Web.Pages.Operaciones.Mesero.IndexModel;

namespace LaMesaDelDuque.Pruebas.Web;

public class MeseroPageTests
{
    [Fact]
    public void MeseroPageModel_NoDebeExponerHandlerDePago()
    {
        var source = ReadWebFile("Pages", "Operaciones", "Mesero", "Index.cshtml.cs");

        Assert.DoesNotContain("OnPostPagarJsonAsync", source);
        Assert.DoesNotContain("Cobrar en mesa", source);
    }

    [Fact]
    public void MeseroJs_NoDebePermitirCobrarDesdePantallaMesero()
    {
        var source = ReadWebFile("wwwroot", "js", "mesero.js");

        Assert.Contains("Cuenta enviada a caja", source);
        Assert.Contains("lmd-mesero-cuenta-en-caja", source);
        Assert.DoesNotContain("PagarJson", source);
        Assert.DoesNotContain("abrirPago", source);
        Assert.DoesNotContain("pagarDirecto", source);
    }

    [Fact]
    public void MeseroPage_DebeTenerZonaVisibleDeMensajes()
    {
        var source = ReadWebFile("Pages", "Operaciones", "Mesero", "Index.cshtml");

        Assert.Contains("lmd-mesero-toast-zone", source);
        Assert.Contains("aria-live=\"polite\"", source);
    }

    [Fact]
    public void MeseroJs_PedirCuentaDebeTenerConfirmacionRobustaYNoEditarItemsEnviados()
    {
        var source = ReadWebFile("wwwroot", "js", "mesero.js");

        Assert.Contains("confirmarAccion", source);
        Assert.Contains("No hay pedido activo", source);
        Assert.Contains("Enviar cuenta a caja", source);
        Assert.Contains("lmd-mesero-item__enviado", source);
        Assert.DoesNotContain("onclick=\"mesero.ajustarCantidad", source);
        Assert.DoesNotContain("onclick=\"mesero.voidItem", source);
    }

    [Fact]
    public void MeseroJs_DebeAgruparMesasPorEstadoOperativo()
    {
        var source = ReadWebFile("wwwroot", "js", "mesero.js");

        Assert.Contains("En caja", source);
        Assert.Contains("Ocupadas", source);
        Assert.Contains("Disponibles", source);
        Assert.Contains("No disponibles", source);
        Assert.Contains("lmd-mesero-mesa-section", source);
        Assert.DoesNotContain("CambiarEstado", source);
    }

    [Fact]
    public void MeseroCss_DebeMejorarContrasteDeProductosYPedidos()
    {
        var source = ReadWebFile("wwwroot", "css", "mesero.css");

        Assert.Contains(".lmd-pos-product-card", source);
        Assert.Contains("background: #f7f4ec", source);
        Assert.Contains(".lmd-mesero-item__enviado", source);
        Assert.Contains(".lmd-mesero-toast-zone", source);
        Assert.Contains(".lmd-mesero-cuenta-en-caja", source);
        Assert.Contains(".lmd-mesero-mesa-section", source);
        Assert.Contains(".lmd-mesero-secciones", source);
    }

    private static MeseroIndexModel CreatePage(FakeMeseroPedidosServicio pedidos) =>
        new(pedidos, new FakeMeseroCatalogoServicio(), new FakeMeseroMesasServicio(), new FakeMeseroHubContext<PedidosHub>(), NullLogger<MeseroIndexModel>.Instance);

    private static string ReadWebFile(params string[] segments) =>
        File.ReadAllText(Path.Combine([ProjectPaths.RepoRoot, "src", "LaMesaDelDuque.Web", .. segments]));

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

        public Task<PedidoDto> CrearPedidoAsync(TipoServicio tipoServicio, Guid? mesaId, List<DetalleCreacionDto> detalles, CancellationToken cancelacion = default, DatosEntregaDto? datosEntrega = null) => Task.FromResult(Pedido);
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
        public Task<List<CategoriaProductoDto>> ListarCategoriasAsync(CancellationToken cancelacion = default) => Task.FromResult(new List<CategoriaProductoDto>());
        public Task<CategoriaProductoDto> CrearCategoriaAsync(string nombre, CancellationToken cancelacion = default) => throw new NotImplementedException();
        public Task<CategoriaProductoDto> ActualizarCategoriaAsync(Guid categoriaId, string nombre, CancellationToken cancelacion = default) => throw new NotImplementedException();
        public Task DesactivarCategoriaAsync(Guid categoriaId, CancellationToken cancelacion = default) => throw new NotImplementedException();
        public Task<List<ProductoDto>> ListarProductosAsync(CancellationToken cancelacion = default) => Task.FromResult(new List<ProductoDto>());
        public Task<List<ProductoDto>> ListarProductosPorCategoriaAsync(Guid categoriaId, CancellationToken cancelacion = default) => Task.FromResult(new List<ProductoDto>());
        public Task<ProductoDto> CrearProductoAsync(string nombre, decimal precio, Guid categoriaId, string? descripcion = null, string? imagenUrl = null, int tiempoPreparacionMin = 5, CancellationToken cancelacion = default) => throw new NotImplementedException();
        public Task<ProductoDto> ActualizarProductoAsync(Guid productoId, string nombre, decimal precio, Guid categoriaId, string? descripcion, string? imagenUrl = null, int? tiempoPreparacionMin = null, CancellationToken cancelacion = default) => throw new NotImplementedException();
        public Task DesactivarProductoAsync(Guid productoId, CancellationToken cancelacion = default) => throw new NotImplementedException();
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
