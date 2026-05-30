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

public sealed class DespachoMesaTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly LaMesaDelDuqueDbContext _contexto;
    private readonly IUnidadDeTrabajo _uot;
    private readonly IDespachoServicio _despacho;

    public DespachoMesaTests()
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
            new PagoRepositorio(_contexto));

        _despacho = new DespachoServicio(_uot);
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

    private async Task<Pedido> CrearPedidoListoConMesaAsync()
    {
        var (mesa, producto) = await CrearMesaYProductoAsync(50);
        mesa.Ocupar();

        var pedido = new Pedido(TipoServicio.ComerAqui, mesa);
        pedido.AgregarDetalle(new DetallePedido(producto, 1, 3.50m, null, null));
        pedido.MarcarEnPreparacion();
        pedido.MarcarComoPagado();
        pedido.MarcarListo();

        await _uot.Pedidos.AgregarAsync(pedido);
        await _uot.GuardarCambiosAsync();

        return pedido;
    }

    [Fact]
    public async Task PagarPedido_ComerAqui_NoLiberaMesaHastaDespacho()
    {
        var (mesa, producto) = await CrearMesaYProductoAsync(51);
        mesa.Ocupar();

        var pedido = new Pedido(TipoServicio.ComerAqui, mesa);
        pedido.AgregarDetalle(new DetallePedido(producto, 1, 3.50m, null, null));
        pedido.MarcarEnPreparacion();
        pedido.MarcarComoPagado();

        await _uot.Pedidos.AgregarAsync(pedido);
        await _uot.GuardarCambiosAsync();

        var mesaActualizada = await _uot.Mesas.ObtenerPorIdAsync(mesa.Id);
        Assert.NotNull(mesaActualizada);
        Assert.Equal(EstadoMesa.Ocupada, mesaActualizada!.Estado);
    }

    [Fact]
    public async Task DespacharPedido_ComerAqui_LiberaMesa()
    {
        var pedido = await CrearPedidoListoConMesaAsync();
        var mesaId = pedido.Mesa!.Id;

        await _despacho.DespacharPedidoAsync(pedido.Id);

        var pedidoActualizado = await _uot.Pedidos.ObtenerConDetallesAsync(pedido.Id);
        var mesaActualizada = await _uot.Mesas.ObtenerPorIdAsync(mesaId);

        Assert.NotNull(pedidoActualizado);
        Assert.Equal(EstadoPedido.Despachado, pedidoActualizado!.Estado);
        Assert.NotNull(mesaActualizada);
        Assert.Equal(EstadoMesa.Disponible, mesaActualizada!.Estado);
    }

    [Fact]
    public async Task DespacharPedido_NoListo_DebeLanzarExcepcion()
    {
        var (mesa, producto) = await CrearMesaYProductoAsync(52);
        mesa.Ocupar();

        var pedido = new Pedido(TipoServicio.ComerAqui, mesa);
        pedido.AgregarDetalle(new DetallePedido(producto, 1, 3.50m, null, null));
        // No marcar como pagado ni listo — queda en Pendiente, que no es despachable

        await _uot.Pedidos.AgregarAsync(pedido);
        await _uot.GuardarCambiosAsync();

        await Assert.ThrowsAsync<ReglaDominioException>(() => _despacho.DespacharPedidoAsync(pedido.Id));
    }

    [Fact]
    public async Task DespacharPedido_Pagado_LiberaMesa()
    {
        var (mesa, producto) = await CrearMesaYProductoAsync(53);
        mesa.Ocupar();

        var pedido = new Pedido(TipoServicio.ComerAqui, mesa);
        pedido.AgregarDetalle(new DetallePedido(producto, 1, 3.50m, null, null));
        pedido.MarcarComoPagado();

        await _uot.Pedidos.AgregarAsync(pedido);
        await _uot.GuardarCambiosAsync();

        await _despacho.DespacharPedidoAsync(pedido.Id);

        var pedidoActualizado = await _uot.Pedidos.ObtenerConDetallesAsync(pedido.Id);
        var mesaActualizada = await _uot.Mesas.ObtenerPorIdAsync(mesa.Id);

        Assert.NotNull(pedidoActualizado);
        Assert.Equal(EstadoPedido.Despachado, pedidoActualizado!.Estado);
        Assert.Equal(EstadoMesa.Disponible, mesaActualizada!.Estado);
    }
}
