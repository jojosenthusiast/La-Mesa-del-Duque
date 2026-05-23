using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using LaMesaDelDuque.Infraestructura.Repositorios;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Pruebas.Persistencia;

public class MetricaRepositorioTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly LaMesaDelDuqueDbContext _contexto;
    private readonly IMetricaRepositorio _metricaRepo;

    public MetricaRepositorioTests()
    {
        _conexion = new SqliteConnection("Data Source=:memory:");
        _conexion.Open();

        var opciones = new DbContextOptionsBuilder<LaMesaDelDuqueDbContext>()
            .UseSqlite(_conexion)
            .Options;

        _contexto = new LaMesaDelDuqueDbContext(opciones);
        _contexto.Database.EnsureCreated();

        _metricaRepo = new MetricaRepositorio(_contexto);
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
    }

    [Fact]
    public async Task ObtenerMetricasHoyAsync_SinDatos_RetornaCeros()
    {
        var inicioTurno = DateTime.UtcNow.Date;

        var metricas = await _metricaRepo.ObtenerMetricasHoyAsync(inicioTurno);

        Assert.Equal(0m, metricas.VentasHoy);
        Assert.Equal(0, metricas.MesasActivas);
        Assert.Equal(0, metricas.TotalMesas);
        Assert.Equal(0m, metricas.TurnoverRate);
        Assert.Equal(0, metricas.PedidosExcedenSLA);
    }

    [Fact]
    public async Task ObtenerMetricasHoyAsync_ConPedidos_RetornaVentasCorrectas()
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
        _contexto.Set<Pedido>().Add(pedido);
        await _contexto.SaveChangesAsync();

        var inicioTurno = DateTime.UtcNow.Date;

        var metricas = await _metricaRepo.ObtenerMetricasHoyAsync(inicioTurno);

        Assert.Equal(7.00m, metricas.VentasHoy);
        Assert.Equal(1, metricas.MesasActivas);
        Assert.Equal(1, metricas.TotalMesas);
    }

    [Fact]
    public async Task ObtenerMetricasHoyAsync_PedidoCancelado_SeExcluyeDeVentas()
    {
        var mesa = new Mesa(2, 4);
        var categoria = new CategoriaProducto("Bebidas");
        var producto = new Producto("Café", 3.50m, categoria);

        _contexto.Set<Mesa>().Add(mesa);
        _contexto.Set<CategoriaProducto>().Add(categoria);
        _contexto.Set<Producto>().Add(producto);
        await _contexto.SaveChangesAsync();

        var pedido = new Pedido(TipoServicio.ComerAqui, mesa);
        pedido.AgregarDetalle(new DetallePedido(producto, 1, 3.50m));
        pedido.Cancelar();
        _contexto.Set<Pedido>().Add(pedido);
        await _contexto.SaveChangesAsync();

        var inicioTurno = DateTime.UtcNow.Date;

        var metricas = await _metricaRepo.ObtenerMetricasHoyAsync(inicioTurno);

        Assert.Equal(0m, metricas.VentasHoy);
        Assert.Equal(0, metricas.MesasActivas);
    }

    [Fact]
    public async Task ObtenerMetricasHoyAsync_PedidoPagado_NoCuentaComoMesaActiva()
    {
        var mesa = new Mesa(3, 4);
        var categoria = new CategoriaProducto("Bebidas");
        var producto = new Producto("Café", 3.50m, categoria);

        _contexto.Set<Mesa>().Add(mesa);
        _contexto.Set<CategoriaProducto>().Add(categoria);
        _contexto.Set<Producto>().Add(producto);
        await _contexto.SaveChangesAsync();

        var pedido = new Pedido(TipoServicio.ComerAqui, mesa);
        pedido.AgregarDetalle(new DetallePedido(producto, 1, 3.50m));
        pedido.MarcarEnPreparacion();
        pedido.MarcarEnCobro();
        pedido.MarcarComoPagado();
        _contexto.Set<Pedido>().Add(pedido);
        await _contexto.SaveChangesAsync();

        var inicioTurno = DateTime.UtcNow.Date;

        var metricas = await _metricaRepo.ObtenerMetricasHoyAsync(inicioTurno);

        Assert.Equal(3.50m, metricas.VentasHoy);
        Assert.Equal(0, metricas.MesasActivas);
    }

    [Fact]
    public async Task ObtenerMetricasHoyAsync_TurnoverRate_CalculadoCorrectamente()
    {
        var mesa1 = new Mesa(4, 4);
        var mesa2 = new Mesa(5, 2);
        var categoria = new CategoriaProducto("Bebidas");
        var producto = new Producto("Café", 3.50m, categoria);

        _contexto.Set<Mesa>().AddRange(mesa1, mesa2);
        _contexto.Set<CategoriaProducto>().Add(categoria);
        _contexto.Set<Producto>().Add(producto);
        await _contexto.SaveChangesAsync();

        var pedido1 = new Pedido(TipoServicio.ComerAqui, mesa1);
        pedido1.AgregarDetalle(new DetallePedido(producto, 1, 3.50m));
        _contexto.Set<Pedido>().Add(pedido1);

        var pedido2 = new Pedido(TipoServicio.ComerAqui, mesa2);
        pedido2.AgregarDetalle(new DetallePedido(producto, 2, 3.50m));
        _contexto.Set<Pedido>().Add(pedido2);

        await _contexto.SaveChangesAsync();

        var inicioTurno = DateTime.UtcNow.Date;

        var metricas = await _metricaRepo.ObtenerMetricasHoyAsync(inicioTurno);

        Assert.Equal(1m, metricas.TurnoverRate); // 2 pedidos / 2 mesas
    }

    [Fact]
    public async Task ObtenerMetricasHoyAsync_SLAExcedido_CuentaCorrectamente()
    {
        var mesa = new Mesa(6, 4);
        var categoria = new CategoriaProducto("Comidas");
        var producto = new Producto("Filete", 15.00m, categoria);

        _contexto.Set<Mesa>().Add(mesa);
        _contexto.Set<CategoriaProducto>().Add(categoria);
        _contexto.Set<Producto>().Add(producto);
        await _contexto.SaveChangesAsync();

        var pedido = new Pedido(TipoServicio.ComerAqui, mesa);
        pedido.AgregarDetalle(new DetallePedido(producto, 1, 15.00m));
        _contexto.Set<Pedido>().Add(pedido);
        await _contexto.SaveChangesAsync();

        // Crear orden de cocina con HoraRecibido hace más de 30 minutos
        var ordenCocina = new OrdenCocina(
            pedido.Id,
            null,
            "Filete",
            1,
            EstacionCocina.Caliente,
            mesa.Numero,
            TipoServicio.ComerAqui.ToString());

        // Usar reflection para establecer HoraRecibido en el pasado
        var propiedad = typeof(OrdenCocina).GetProperty("HoraRecibido")!;
        propiedad.SetValue(ordenCocina, DateTime.UtcNow.AddMinutes(-45));

        ordenCocina.MarcarEnPreparacion();
        _contexto.Set<OrdenCocina>().Add(ordenCocina);
        await _contexto.SaveChangesAsync();

        var inicioTurno = DateTime.UtcNow.Date;

        var metricas = await _metricaRepo.ObtenerMetricasHoyAsync(inicioTurno);

        Assert.Equal(1, metricas.PedidosExcedenSLA);
    }
}
