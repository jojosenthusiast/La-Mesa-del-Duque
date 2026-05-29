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

public class PedidosServicioDashboardTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly LaMesaDelDuqueDbContext _contexto;
    private readonly IUnidadDeTrabajo _uot;
    private readonly NotificadorPedidosSpy _notificadorPedidosSpy;
    private readonly NotificadorDashboardSpy _notificadorDashboardSpy;
    private readonly IPedidosServicio _servicio;

    public PedidosServicioDashboardTests()
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
            new PromocionRepositorio(_contexto),
            new TurnoCajaRepositorio(_contexto),
            new DescuentoRepositorio(_contexto),
            new MotivoDescuentoRepositorio(_contexto),
            new DevolucionRepositorio(_contexto));

        _notificadorPedidosSpy = new NotificadorPedidosSpy();
        _notificadorDashboardSpy = new NotificadorDashboardSpy();
        _servicio = new PedidosServicio(
            _uot,
            _notificadorPedidosSpy,
            httpContextAccessor: TestHttpContextAccessor.ConUsuarioAutenticado(),
            notificadorDashboard: _notificadorDashboardSpy);
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

    [Fact]
    public async Task CrearPedidoAsync_EmiteNotificacionDashboard()
    {
        var (mesa, producto) = await CrearMesaYProductoAsync();
        var detalles = new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 2, PrecioUnitario = 3.50m }
        };

        await _servicio.CrearPedidoAsync(TipoServicio.ComerAqui, mesa.Id, detalles);

        Assert.Equal(1, _notificadorDashboardSpy.MetricasInvalidadasCount);
    }

    [Fact]
    public async Task EstadoEnPreparacion_AutoTransicion_EmiteNotificacionDashboard()
    {
        var (mesa, producto) = await CrearMesaYProductoAsync();
        var detalles = new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 3.50m }
        };

        await _servicio.CrearPedidoAsync(TipoServicio.ComerAqui, mesa.Id, detalles);

        Assert.Equal(1, _notificadorDashboardSpy.MetricasInvalidadasCount);
    }

    [Fact]
    public async Task PagarPedidoAsync_EmiteNotificacionDashboard()
    {
        var (mesa, producto) = await CrearMesaYProductoAsync();
        var detalles = new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 3.50m }
        };

        var pedido = await _servicio.CrearPedidoAsync(TipoServicio.ComerAqui, mesa.Id, detalles);
        await _servicio.MarcarEnCobroAsync(pedido.Id);
        _notificadorDashboardSpy.Reset();

        await _servicio.PagarPedidoAsync(pedido.Id);

        Assert.Equal(1, _notificadorDashboardSpy.MetricasInvalidadasCount);
    }

    [Fact]
    public async Task CancelarPedidoAsync_EmiteNotificacionDashboard()
    {
        var (mesa, producto) = await CrearMesaYProductoAsync();
        var detalles = new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 3.50m }
        };

        var pedido = await _servicio.CrearPedidoAsync(TipoServicio.ComerAqui, mesa.Id, detalles);
        _notificadorDashboardSpy.Reset();

        await _servicio.CancelarPedidoAsync(pedido.Id);

        Assert.Equal(1, _notificadorDashboardSpy.MetricasInvalidadasCount);
    }

    [Fact]
    public async Task MarcarEnCobroAsync_EmiteNotificacionDashboard()
    {
        var (mesa, producto) = await CrearMesaYProductoAsync();
        var detalles = new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 3.50m }
        };

        var pedido = await _servicio.CrearPedidoAsync(TipoServicio.ComerAqui, mesa.Id, detalles);
        _notificadorDashboardSpy.Reset();

        await _servicio.MarcarEnCobroAsync(pedido.Id);

        Assert.Equal(1, _notificadorDashboardSpy.MetricasInvalidadasCount);
    }

    [Fact]
    public async Task CrearCuentasAsync_EmiteNotificacionDashboard()
    {
        var (mesa, producto) = await CrearMesaYProductoAsync();
        var detalles = new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 2, PrecioUnitario = 3.50m }
        };

        var pedido = await _servicio.CrearPedidoAsync(TipoServicio.ComerAqui, mesa.Id, detalles);
        _notificadorDashboardSpy.Reset();

        await _servicio.CrearCuentasAsync(pedido.Id, 2);

        Assert.Equal(1, _notificadorDashboardSpy.MetricasInvalidadasCount);
    }

    [Fact]
    public async Task PedidosServicio_SinNotificadorDashboard_NoLanzaExcepcion()
    {
        var (mesa, producto) = await CrearMesaYProductoAsync();
        var detalles = new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 3.50m }
        };

        var servicioSinDashboard = new PedidosServicio(
            _uot,
            _notificadorPedidosSpy,
            httpContextAccessor: TestHttpContextAccessor.ConUsuarioAutenticado());

        var pedido = await servicioSinDashboard.CrearPedidoAsync(TipoServicio.ComerAqui, mesa.Id, detalles);
        await servicioSinDashboard.MarcarEnCobroAsync(pedido.Id);
        await servicioSinDashboard.PagarPedidoAsync(pedido.Id);

        // No exception thrown = test passes
        Assert.True(true);
    }
}
