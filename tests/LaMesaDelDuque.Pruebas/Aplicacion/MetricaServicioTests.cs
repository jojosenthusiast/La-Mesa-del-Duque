using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Modelos;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using LaMesaDelDuque.Infraestructura.Repositorios;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Pruebas.Aplicacion;

public class MetricaServicioTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly LaMesaDelDuqueDbContext _contexto;
    private readonly IMetricaRepositorio _metricaRepo;
    private readonly IMetricaServicio _servicio;

    public MetricaServicioTests()
    {
        _conexion = new SqliteConnection("Data Source=:memory:");
        _conexion.Open();

        var opciones = new DbContextOptionsBuilder<LaMesaDelDuqueDbContext>()
            .UseSqlite(_conexion)
            .Options;

        _contexto = new LaMesaDelDuqueDbContext(opciones);
        _contexto.Database.EnsureCreated();

        _metricaRepo = new MetricaRepositorio(_contexto);
        _servicio = new MetricaServicio(_metricaRepo);
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
    }

    [Fact]
    public async Task ObtenerMetricasOperativasAsync_SinDatos_RetornaCeros()
    {
        var metricas = await _servicio.ObtenerMetricasOperativasAsync();

        Assert.Equal(0m, metricas.VentasHoy);
        Assert.Equal(0, metricas.MesasActivas);
        Assert.Equal(0, metricas.TotalMesas);
        Assert.Equal(0m, metricas.TurnoverRate);
        Assert.Equal(0, metricas.PedidosExcedenSLA);
    }

    [Fact]
    public async Task ObtenerMetricasOperativasAsync_ConPedidoPagado_RetornaVentasYMesaDisponible()
    {
        var mesa = new Mesa(1, 4);
        var categoria = new CategoriaProducto("Bebidas");
        var producto = new Producto("Café", 3.50m, categoria);

        _contexto.Set<Mesa>().Add(mesa);
        _contexto.Set<CategoriaProducto>().Add(categoria);
        _contexto.Set<Producto>().Add(producto);
        await _contexto.SaveChangesAsync();

        var pedido = new Pedido(TipoServicio.ComerAqui, mesa);
        pedido.AgregarDetalle(new DetallePedido(producto, 2, 3.50m));
        pedido.MarcarEnPreparacion();
        pedido.MarcarEnCobro();
        var cuenta = pedido.CrearCuentas(1).Single();
        var usuarioId = Guid.NewGuid();
        cuenta.Pagar(MetodoPago.Efectivo, usuarioId: usuarioId);
        pedido.MarcarComoPagado();
        _contexto.Set<Pedido>().Add(pedido);
        _contexto.Set<Pago>().Add(new Pago(cuenta.Id, cuenta.Total, MetodoPago.Efectivo, usuarioId: usuarioId));
        await _contexto.SaveChangesAsync();

        var metricas = await _servicio.ObtenerMetricasOperativasAsync();

        Assert.Equal(7.00m, metricas.VentasHoy);
        Assert.Equal(0, metricas.MesasActivas); // pagado = no activa
        Assert.Equal(1, metricas.TotalMesas);
        Assert.Equal(1m, metricas.TurnoverRate);
    }

    [Fact]
    public async Task ObtenerMetricasOperativasAsync_ConPedidoActivo_RetornaMesaActiva()
    {
        var mesa = new Mesa(2, 4);
        var categoria = new CategoriaProducto("Comidas");
        var producto = new Producto("Filete", 15.00m, categoria);

        _contexto.Set<Mesa>().Add(mesa);
        _contexto.Set<CategoriaProducto>().Add(categoria);
        _contexto.Set<Producto>().Add(producto);
        await _contexto.SaveChangesAsync();

        var pedido = new Pedido(TipoServicio.ComerAqui, mesa);
        pedido.AgregarDetalle(new DetallePedido(producto, 1, 15.00m));
        pedido.MarcarEnPreparacion();
        _contexto.Set<Pedido>().Add(pedido);
        await _contexto.SaveChangesAsync();

        var metricas = await _servicio.ObtenerMetricasOperativasAsync();

        Assert.Equal(0m, metricas.VentasHoy);
        Assert.Equal(1, metricas.MesasActivas);
    }
}
