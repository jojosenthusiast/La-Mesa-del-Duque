using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using LaMesaDelDuque.Infraestructura.Repositorios;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Pruebas.Aplicacion;

public class CuentaServicioTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly LaMesaDelDuqueDbContext _contexto;
    private readonly IPedidosServicio _servicio;
    private readonly IUnidadDeTrabajo _uot;
    private readonly NotificadorPedidosSpy _notificadorSpy;

    public CuentaServicioTests()
    {
        _conexion = new SqliteConnection("Data Source=:memory:");
        _conexion.Open();

        var opciones = new DbContextOptionsBuilder<LaMesaDelDuqueDbContext>()
            .UseSqlite(_conexion)
            .Options;

        _contexto = new LaMesaDelDuqueDbContext(opciones);
        _contexto.Database.EnsureCreated();

        _uot = new UnidadDeTrabajo(_contexto,
            new CategoriaProductoRepositorio(_contexto),
            new ProductoRepositorio(_contexto),
            new IngredienteRepositorio(_contexto),
            new MesaRepositorio(_contexto),
            new PedidoRepositorio(_contexto),
            new RolRepositorio(_contexto),
            new UsuarioRepositorio(_contexto),
            new AuditoriaRepositorio(_contexto),
            new RecetaProductoRepositorio(_contexto),
            new OrdenCocinaRepositorio(_contexto),
            new CuentaRepositorio(_contexto),
            new PagoRepositorio(_contexto),
            new ZonaSalonRepositorio(_contexto));

        _notificadorSpy = new NotificadorPedidosSpy();
        _servicio = new PedidosServicio(_uot, _notificadorSpy);
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
    }

    private async Task<(Mesa mesa, Producto producto)> CrearMesaYProductoAsync(int numeroMesa = 1)
    {
        var mesa = new Mesa(numeroMesa, 4);
        var categoria = new CategoriaProducto($"Bebidas {numeroMesa}");
        var producto = new Producto($"Café {numeroMesa}", 3.50m, categoria);

        await _uot.Mesas.AgregarAsync(mesa);
        await _uot.Categorias.AgregarAsync(categoria);
        await _uot.Productos.AgregarAsync(producto);
        await _uot.GuardarCambiosAsync();

        return (mesa, producto);
    }

    private async Task<PedidoDto> CrearPedidoEnPreparacionAsync()
    {
        var (mesa, producto) = await CrearMesaYProductoAsync(50);
        var pedido = await _servicio.CrearPedidoAsync(TipoServicio.ComerAqui, mesa.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 2, PrecioUnitario = 10.00m }
        });
        await _servicio.MarcarEnPreparacionAsync(pedido.Id);
        return pedido;
    }

    [Fact]
    public async Task CrearCuentasAsync_PedidoEnPreparacion_DebeDividirTotalYMarcarEnCobro()
    {
        var pedido = await CrearPedidoEnPreparacionAsync();

        var cuentas = await _servicio.CrearCuentasAsync(pedido.Id, 2);

        Assert.Equal(2, cuentas.Count);
        Assert.Equal(10.00m, cuentas[0].Total);
        Assert.Equal(10.00m, cuentas[1].Total);
        Assert.Equal("EnCobro", (await _servicio.ObtenerPedidoAsync(pedido.Id))!.Estado);
    }

    [Fact]
    public async Task CrearCuentasAsync_DebeEmitirNotificacionDeCambioEstado()
    {
        var pedido = await CrearPedidoEnPreparacionAsync();
        _notificadorSpy.EstadosCambiados.Clear();

        await _servicio.CrearCuentasAsync(pedido.Id, 2);

        var notificacion = Assert.Single(_notificadorSpy.EstadosCambiados);
        Assert.Equal(pedido.Id, notificacion.PedidoId);
        Assert.Equal(EstadoPedido.EnCobro, notificacion.Estado);
    }

    [Fact]
    public async Task PagarCuentaAsync_CuentaAbierta_DebeMarcarComoPagada()
    {
        var pedido = await CrearPedidoEnPreparacionAsync();
        var cuentas = await _servicio.CrearCuentasAsync(pedido.Id, 2);

        var pagada = await _servicio.PagarCuentaAsync(cuentas[0].Id, MetodoPago.Efectivo, 1.50m);

        Assert.Equal("Pagada", pagada.Estado);
        Assert.Equal("Efectivo", pagada.MetodoPago);
        Assert.Equal(1.50m, pagada.PropinaMonto);
    }

    [Fact]
    public async Task PagarCuentaAsync_UltimaCuentaPendiente_DebeMarcarPedidoPagadoYLiberarMesa()
    {
        var pedido = await CrearPedidoEnPreparacionAsync();
        var cuentas = await _servicio.CrearCuentasAsync(pedido.Id, 2);
        await _servicio.PagarCuentaAsync(cuentas[0].Id, MetodoPago.Efectivo);

        var pagada = await _servicio.PagarCuentaAsync(cuentas[1].Id, MetodoPago.Tarjeta);
        var pedidoActualizado = await _servicio.ObtenerPedidoAsync(pedido.Id);
        var mesa = await _uot.Mesas.ObtenerPorIdAsync(pedidoActualizado!.MesaId!.Value);

        Assert.Equal("Pagada", pagada.Estado);
        Assert.Equal("Pagado", pedidoActualizado.Estado);
        Assert.NotNull(mesa);
        Assert.Equal(EstadoMesa.Disponible, mesa!.Estado);
    }

    [Fact]
    public async Task PagarCuentaAsync_CuentaInexistente_DebeLanzarExcepcion()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _servicio.PagarCuentaAsync(Guid.NewGuid(), MetodoPago.Efectivo));
    }

    [Fact]
    public async Task ObtenerCuentasAsync_DebeRetornarCuentasDelPedido()
    {
        var pedido = await CrearPedidoEnPreparacionAsync();
        await _servicio.CrearCuentasAsync(pedido.Id, 3);

        var cuentas = await _servicio.ObtenerCuentasAsync(pedido.Id);

        Assert.Equal(3, cuentas.Count);
        Assert.All(cuentas, c => Assert.Equal(pedido.Id, c.PedidoId));
    }

    [Fact]
    public async Task PagarCuentaAsync_DosVecesMismaCuenta_DebeLanzarExcepcion()
    {
        var pedido = await CrearPedidoEnPreparacionAsync();
        var cuentas = await _servicio.CrearCuentasAsync(pedido.Id, 1);
        await _servicio.PagarCuentaAsync(cuentas[0].Id, MetodoPago.Efectivo);

        await Assert.ThrowsAsync<ReglaDominioException>(() => _servicio.PagarCuentaAsync(cuentas[0].Id, MetodoPago.Tarjeta));
    }

    [Fact]
    public async Task CrearCuentasConItemsAsync_DosCuentasConItemsDistintos_DebeCrearCorrectamente()
    {
        var (mesa, producto) = await CrearMesaYProductoAsync(60);
        var producto2 = new Producto("Ensalada", 8.00m, new CategoriaProducto("Entradas 60"));
        await _uot.Categorias.AgregarAsync(producto2.Categoria);
        await _uot.Productos.AgregarAsync(producto2);
        await _uot.GuardarCambiosAsync();

        var pedido = await _servicio.CrearPedidoAsync(TipoServicio.ComerAqui, mesa.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 25.00m },
            new() { ProductoId = producto2.Id, Cantidad = 1, PrecioUnitario = 8.00m }
        });
        await _servicio.MarcarEnPreparacionAsync(pedido.Id);

        var asignaciones = new Dictionary<int, List<(Guid detalleId, int cantidad)>>
        {
            [1] = new() { (pedido.Detalles[0].Id, 1) },
            [2] = new() { (pedido.Detalles[1].Id, 1) }
        };

        var cuentas = await _servicio.CrearCuentasConItemsAsync(pedido.Id, asignaciones);

        Assert.Equal(2, cuentas.Count);
        Assert.Equal(25.00m, cuentas[0].Total);
        Assert.Equal(8.00m, cuentas[1].Total);
        Assert.Single(cuentas[0].Detalles);
        Assert.Single(cuentas[1].Detalles);
    }

    [Fact]
    public async Task CrearCuentasConItemsAsync_CantidadParcial_DebeDividirItem()
    {
        var pedido = await CrearPedidoEnPreparacionAsync();
        var detalleId = pedido.Detalles[0].Id;

        var asignaciones = new Dictionary<int, List<(Guid detalleId, int cantidad)>>
        {
            [1] = new() { (detalleId, 1) },
            [2] = new() { (detalleId, 1) }
        };

        var cuentas = await _servicio.CrearCuentasConItemsAsync(pedido.Id, asignaciones);

        Assert.Equal(2, cuentas.Count);
        Assert.Equal(10.00m, cuentas[0].Total);
        Assert.Equal(10.00m, cuentas[1].Total);
    }

    [Fact]
    public async Task CrearCuentasConItemsAsync_MenosDeDosCuentas_DebeLanzarExcepcion()
    {
        var pedido = await CrearPedidoEnPreparacionAsync();

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _servicio.CrearCuentasConItemsAsync(pedido.Id, new Dictionary<int, List<(Guid, int)>>()));

        Assert.Contains("al menos 2", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
