using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Notificaciones;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using LaMesaDelDuque.Infraestructura.Repositorios;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Pruebas.Aplicacion;

public class CocinaServicioTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly LaMesaDelDuqueDbContext _contexto;
    private readonly IUnidadDeTrabajo _uot;
    private readonly NotificadorPedidosSpy _notificadorSpy;
    private readonly ICocinaServicio _servicio;

    public CocinaServicioTests()
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

        var rolCaja = new Rol("Cajero");
        var usuarioCaja = new Usuario("cajero-cocina", "cajero-cocina@lmd.test", "hash-demo", "Cajero Cocina", rolCaja);
        _contexto.Set<Rol>().Add(rolCaja);
        _contexto.Set<Usuario>().Add(usuarioCaja);
        _contexto.Set<CierreDia>().Add(new CierreDia(DateOnly.FromDateTime(DateTime.UtcNow), 0, 0, 0, 0, 0, 0, usuarioCaja));
        _contexto.Set<TurnoCaja>().Add(new TurnoCaja(usuarioCaja.Id, 100m));
        _contexto.SaveChanges();

        _notificadorSpy = new NotificadorPedidosSpy();
        _servicio = new CocinaServicio(_uot, _notificadorSpy);
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
    }

    private async Task<(Mesa mesa, Producto producto)> CrearMesaYProductoAsync(EstacionCocina estacion = EstacionCocina.Parrilla)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var mesaNumero = new Random().Next(1000, 9999);
        var mesa = new Mesa(mesaNumero, 4);
        var categoria = new CategoriaProducto($"Test-{suffix}", estacionCocina: estacion);
        var producto = new Producto($"Test Producto {suffix}", 10m, categoria);

        await _uot.Mesas.AgregarAsync(mesa);
        await _uot.Categorias.AgregarAsync(categoria);
        await _uot.Productos.AgregarAsync(producto);
        await _uot.GuardarCambiosAsync();

        return (mesa, producto);
    }

    private async Task<Pedido> CrearPedidoAsync(Mesa? mesa = null, Producto? producto = null)
    {
        if (producto is null)
        {
            (_, producto) = await CrearMesaYProductoAsync();
        }

        var pedido = new Pedido(mesa is null ? TipoServicio.ParaLlevar : TipoServicio.ComerAqui, mesa);
        pedido.AgregarDetalle(new DetallePedido(producto, 2, 10m));

        await _uot.Pedidos.AgregarAsync(pedido);
        await _uot.GuardarCambiosAsync();

        return pedido;
    }

    [Fact]
    public async Task GenerarOrdenesAsync_CuandoPedidoTieneDetalles_DebeCrearOrdenesCocina()
    {
        var (mesa, producto) = await CrearMesaYProductoAsync(EstacionCocina.Parrilla);
        var pedido = await CrearPedidoAsync(mesa, producto);

        await _servicio.GenerarOrdenesAsync(pedido.Id);

        var ordenes = await _contexto.Set<OrdenCocina>().Where(o => o.PedidoId == pedido.Id).ToListAsync();
        Assert.Single(ordenes);
        Assert.Equal(producto.Nombre, ordenes[0].ProductoNombre);
        Assert.Equal(2, ordenes[0].Cantidad);
        Assert.Equal(EstacionCocina.Parrilla, ordenes[0].Estacion);
        Assert.Equal(EstadoLineaCocina.Pendiente, ordenes[0].Estado);
    }

    [Fact]
    public async Task GenerarOrdenesAsync_DebeEmitirNotificacionPorEstacion()
    {
        var (_, producto) = await CrearMesaYProductoAsync(EstacionCocina.Fria);
        var pedido = await CrearPedidoAsync(null, producto);

        await _servicio.GenerarOrdenesAsync(pedido.Id);

        Assert.Contains(_notificadorSpy.OrdenesCocina, n => n.Estacion == "Fria");
    }

    [Fact]
    public async Task GenerarOrdenesAsync_CuandoDetalleYaTieneOrden_NoDuplicaNiReNotifica()
    {
        var (mesa, producto) = await CrearMesaYProductoAsync(EstacionCocina.Parrilla);
        var pedido = await CrearPedidoAsync(mesa, producto);

        await _servicio.GenerarOrdenesAsync(pedido.Id);
        _notificadorSpy.OrdenesCocina.Clear();

        await _servicio.GenerarOrdenesAsync(pedido.Id);

        var ordenes = await _contexto.Set<OrdenCocina>().Where(o => o.PedidoId == pedido.Id).ToListAsync();
        Assert.Single(ordenes);
        Assert.Empty(_notificadorSpy.OrdenesCocina);
    }

    [Fact]
    public async Task ListarPendientesAsync_SinFiltro_DebeRetornarTodasLasPendientesYNoListos()
    {
        var (_, producto1) = await CrearMesaYProductoAsync(EstacionCocina.Parrilla);
        var (_, producto2) = await CrearMesaYProductoAsync(EstacionCocina.Fria);

        var pedido1 = await CrearPedidoAsync(null, producto1);
        var pedido2 = await CrearPedidoAsync(null, producto2);

        await _servicio.GenerarOrdenesAsync(pedido1.Id);
        await _servicio.GenerarOrdenesAsync(pedido2.Id);

        var ordenes = (await _uot.OrdenesCocina.ListarPendientesAsync()).ToList();
        var orden1 = ordenes.First(o => o.PedidoId == pedido1.Id);
        await _servicio.MarcarListoAsync(orden1.Id);

        var pendientes = await _servicio.ListarPendientesAsync();

        Assert.DoesNotContain(pendientes, o => o.PedidoId == pedido1.Id);
        Assert.Contains(pendientes, o => o.PedidoId == pedido2.Id);
    }

    [Fact]
    public async Task ListarPendientesAsync_NoDebeRetornarCanceladasNiEntregadas()
    {
        var (_, productoPendiente) = await CrearMesaYProductoAsync(EstacionCocina.Parrilla);
        var (_, productoEntregado) = await CrearMesaYProductoAsync(EstacionCocina.Fria);
        var (_, productoCancelado) = await CrearMesaYProductoAsync(EstacionCocina.Caliente);

        var pedidoPendiente = await CrearPedidoAsync(null, productoPendiente);
        var pedidoEntregado = await CrearPedidoAsync(null, productoEntregado);
        var pedidoCancelado = await CrearPedidoAsync(null, productoCancelado);

        await _servicio.GenerarOrdenesAsync(pedidoPendiente.Id);
        await _servicio.GenerarOrdenesAsync(pedidoEntregado.Id);
        await _servicio.GenerarOrdenesAsync(pedidoCancelado.Id);

        var ordenEntregada = await _contexto.Set<OrdenCocina>().FirstAsync(o => o.PedidoId == pedidoEntregado.Id);
        var ordenCancelada = await _contexto.Set<OrdenCocina>().FirstAsync(o => o.PedidoId == pedidoCancelado.Id);
        CambiarEstadoCocina(ordenEntregada, EstadoLineaCocina.Entregado);
        CambiarEstadoCocina(ordenCancelada, EstadoLineaCocina.Cancelado);
        await _uot.GuardarCambiosAsync();

        var pendientes = await _servicio.ListarPendientesAsync();

        Assert.Contains(pendientes, o => o.PedidoId == pedidoPendiente.Id && o.Estado == EstadoLineaCocina.Pendiente.ToString());
        Assert.DoesNotContain(pendientes, o => o.PedidoId == pedidoEntregado.Id);
        Assert.DoesNotContain(pendientes, o => o.PedidoId == pedidoCancelado.Id);
    }

    [Fact]
    public async Task ListarPendientesAsync_ConFiltroEstacion_DebeFiltrar()
    {
        var (_, producto1) = await CrearMesaYProductoAsync(EstacionCocina.Parrilla);
        var (_, producto2) = await CrearMesaYProductoAsync(EstacionCocina.Fria);

        var pedido1 = await CrearPedidoAsync(null, producto1);
        var pedido2 = await CrearPedidoAsync(null, producto2);

        await _servicio.GenerarOrdenesAsync(pedido1.Id);
        await _servicio.GenerarOrdenesAsync(pedido2.Id);

        var parrilla = await _servicio.ListarPendientesAsync(EstacionCocina.Parrilla);

        Assert.All(parrilla, o => Assert.Equal("Parrilla", o.Estacion));
    }

    [Fact]
    public async Task MarcarListoAsync_CuandoOrdenExiste_DebeCambiarEstadoYNotificar()
    {
        var (_, producto) = await CrearMesaYProductoAsync();
        var pedido = await CrearPedidoAsync(null, producto);
        await _servicio.GenerarOrdenesAsync(pedido.Id);

        var orden = (await _uot.OrdenesCocina.ListarPendientesAsync()).First();
        _notificadorSpy.ItemsListos.Clear();

        var dto = await _servicio.MarcarListoAsync(orden.Id);

        Assert.Equal(EstadoLineaCocina.Listo.ToString(), dto.Estado);
        Assert.Single(_notificadorSpy.ItemsListos);
        Assert.Equal(orden.Id, _notificadorSpy.ItemsListos[0].OrdenId);
    }

    [Fact]
    public async Task MarcarListoAsync_CuandoPedidoYaEstaPagado_NoDebeVolverAListo()
    {
        var (_, producto) = await CrearMesaYProductoAsync();
        var pedido = await CrearPedidoAsync(null, producto);
        pedido.MarcarEnPreparacion();
        pedido.MarcarEnCobro();
        pedido.MarcarComoPagado();
        await _uot.GuardarCambiosAsync();
        await _servicio.GenerarOrdenesAsync(pedido.Id);

        var orden = (await _uot.OrdenesCocina.ListarPendientesAsync()).First();

        await _servicio.MarcarListoAsync(orden.Id);

        var pedidoActualizado = await _uot.Pedidos.ObtenerConDetallesAsync(pedido.Id);
        Assert.Equal(EstadoPedido.Pagado, pedidoActualizado!.Estado);
    }

    [Fact]
    public async Task RecuperarAsync_CuandoOrdenEstaListo_DebeVolverAEnPreparacion()
    {
        var (_, producto) = await CrearMesaYProductoAsync();
        var pedido = await CrearPedidoAsync(null, producto);
        await _servicio.GenerarOrdenesAsync(pedido.Id);

        var orden = (await _uot.OrdenesCocina.ListarPendientesAsync()).First();
        await _servicio.MarcarListoAsync(orden.Id);
        _notificadorSpy.ItemsRecuperados.Clear();

        var dto = await _servicio.RecuperarAsync(orden.Id);

        Assert.Equal(EstadoLineaCocina.EnPreparacion.ToString(), dto.Estado);
        Assert.Single(_notificadorSpy.ItemsRecuperados);
    }

    [Fact]
    public async Task PedidosServicio_CrearPedidoAsync_ConCocinaServicio_DebeGenerarOrdenesCocina()
    {
        var (mesa, producto) = await CrearMesaYProductoAsync(EstacionCocina.Caliente);
        var pedidosServicio = new PedidosServicio(_uot, _notificadorSpy, _servicio);

        var pedido = await pedidosServicio.CrearPedidoAsync(TipoServicio.ComerAqui, mesa.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 10m }
        });

        var ordenes = await _contexto.Set<OrdenCocina>().Where(o => o.PedidoId == pedido.Id).ToListAsync();
        Assert.Single(ordenes);
        Assert.Equal(EstacionCocina.Caliente, ordenes[0].Estacion);
    }

    private static void CambiarEstadoCocina(OrdenCocina orden, EstadoLineaCocina estado)
    {
        typeof(OrdenCocina)
            .GetProperty(nameof(OrdenCocina.Estado))!
            .SetValue(orden, estado);
    }
}
