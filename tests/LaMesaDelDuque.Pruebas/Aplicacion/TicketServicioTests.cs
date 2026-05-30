using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using LaMesaDelDuque.Infraestructura.Repositorios;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Pruebas.Aplicacion;

public class TicketServicioTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly LaMesaDelDuqueDbContext _contexto;
    private readonly IUnidadDeTrabajo _uot;
    private readonly TicketServicio _servicio;

    public TicketServicioTests()
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

        _servicio = new TicketServicio(_uot);
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
    }

    [Fact]
    public async Task GenerarHtmlTicketAsync_Domicilio_DebeMostrarDatosEntregaEscapados()
    {
        var categoria = new CategoriaProducto("Pizzas");
        var producto = new Producto("Pizza <script>", 15m, categoria);
        var pedido = new Pedido(
            TipoServicio.Domicilio,
            mesa: null,
            nombreClienteEntrega: "Ana <b>",
            telefonoEntrega: "809-555-0199",
            direccionEntrega: "Calle <script> #12",
            referenciaEntrega: "Portón & azul");

        pedido.AgregarDetalle(new DetallePedido(producto, 1, 15m));

        await _uot.Categorias.AgregarAsync(categoria);
        await _uot.Productos.AgregarAsync(producto);
        await _uot.Pedidos.AgregarAsync(pedido);
        await _uot.GuardarCambiosAsync();

        var html = await _servicio.GenerarHtmlTicketAsync(pedido.Id);

        Assert.Contains("<b>Servicio:</b> Domicilio", html);
        Assert.Contains("<b>Cliente:</b> Ana &lt;b&gt;", html);
        Assert.Contains("<b>Dirección:</b> Calle &lt;script&gt; #12", html);
        Assert.Contains("<b>Referencia:</b> Port", html);
        Assert.Contains("&amp; azul", html);
        Assert.DoesNotContain("Ana <b>", html);
        Assert.DoesNotContain("Calle <script> #12", html);
        Assert.DoesNotContain("Portón & azul", html);
        Assert.DoesNotContain("Pizza <script>", html);
    }
}
