using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Modelos;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using LaMesaDelDuque.Infraestructura.Repositorios;
using LaMesaDelDuque.Web.Pages.Admin.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Pruebas.Web;

public class DashboardPageTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly LaMesaDelDuqueDbContext _contexto;
    private readonly IMetricaRepositorio _metricaRepo;
    private readonly IMetricaServicio _metricaServicio;
    private readonly DashboardModel _pageModel;

    public DashboardPageTests()
    {
        _conexion = new SqliteConnection("Data Source=:memory:");
        _conexion.Open();

        var opciones = new DbContextOptionsBuilder<LaMesaDelDuqueDbContext>()
            .UseSqlite(_conexion)
            .Options;

        _contexto = new LaMesaDelDuqueDbContext(opciones);
        _contexto.Database.EnsureCreated();

        _metricaRepo = new MetricaRepositorio(_contexto);
        _metricaServicio = new MetricaServicio(_metricaRepo);
        _pageModel = new DashboardModel(_metricaServicio);
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
    }

    [Fact]
    public async Task OnGetAsync_CargaMetricasCorrectamente()
    {
        await _pageModel.OnGetAsync();

        Assert.NotNull(_pageModel.Metricas);
        Assert.Equal(0m, _pageModel.Metricas.VentasHoy);
    }

    [Fact]
    public async Task OnGetAsync_CargaVentasPorHoraCorrectamente()
    {
        await _pageModel.OnGetAsync();

        Assert.NotNull(_pageModel.VentasPorHora);
        Assert.Equal(24, _pageModel.VentasPorHora.Count);
    }

    [Fact]
    public async Task OnGetMetricasJsonAsync_RetornaJsonResult()
    {
        var result = await _pageModel.OnGetMetricasJsonAsync();

        Assert.IsType<JsonResult>(result);
        var json = (JsonResult)result;
        Assert.IsType<MetricasOperativasDto>(json.Value);
    }

    [Fact]
    public async Task OnGetVentasPorHoraJsonAsync_RetornaJsonResult()
    {
        var result = await _pageModel.OnGetVentasPorHoraJsonAsync();

        Assert.IsType<JsonResult>(result);
        var json = (JsonResult)result;
        var ventas = Assert.IsType<List<VentaPorHoraDto>>(json.Value);
        Assert.Equal(24, ventas.Count);
    }

    [Fact]
    public async Task OnGetAsync_ConDatos_MuestraVentasCorrectas()
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

        await _pageModel.OnGetAsync();

        Assert.Equal(7.00m, _pageModel.Metricas.VentasHoy);
        Assert.Equal(1, _pageModel.Metricas.MesasActivas);
    }

    [Fact]
    public void DashboardModel_TieneAuthorizeAttribute()
    {
        var attribute = typeof(DashboardModel).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .OfType<AuthorizeAttribute>()
            .FirstOrDefault();

        Assert.NotNull(attribute);
        Assert.Equal("Administrador,Encargado", attribute.Roles);
    }

}
