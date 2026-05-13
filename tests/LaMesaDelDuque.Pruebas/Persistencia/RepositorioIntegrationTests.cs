using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using LaMesaDelDuque.Infraestructura.Repositorios;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Pruebas.Persistencia;

public class RepositorioIntegrationTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly LaMesaDelDuqueDbContext _contexto;
    private readonly IUnidadDeTrabajo _uot;

    public RepositorioIntegrationTests()
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
            new OrdenCocinaRepositorio(_contexto));
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
    }

    // --- CategoriaProducto ---

    [Fact]
    public async Task CategoriaRepositorio_AgregarYObtener_DebeFuncionar()
    {
        var categoria = new CategoriaProducto("Bebidas");
        await _uot.Categorias.AgregarAsync(categoria);
        await _uot.GuardarCambiosAsync();

        var recuperada = await _uot.Categorias.ObtenerPorIdAsync(categoria.Id);

        Assert.NotNull(recuperada);
        Assert.Equal("Bebidas", recuperada!.Nombre);
    }

    [Fact]
    public async Task CategoriaRepositorio_ObtenerTodas_DebeRetornarLista()
    {
        await _uot.Categorias.AgregarAsync(new CategoriaProducto("Bebidas"));
        await _uot.Categorias.AgregarAsync(new CategoriaProducto("Entradas"));
        await _uot.GuardarCambiosAsync();

        var categorias = await _uot.Categorias.ObtenerTodasAsync();

        Assert.Equal(2, categorias.Count);
    }

    // --- Producto ---

    [Fact]
    public async Task ProductoRepositorio_AgregarYObtenerConCategoria_DebeFuncionar()
    {
        var categoria = new CategoriaProducto("Entradas");
        await _uot.Categorias.AgregarAsync(categoria);
        await _uot.GuardarCambiosAsync();

        var producto = new Producto("Bruschetta", 8.00m, categoria);
        await _uot.Productos.AgregarAsync(producto);
        await _uot.GuardarCambiosAsync();

        var recuperado = await _uot.Productos.ObtenerPorIdAsync(producto.Id);
        Assert.NotNull(recuperado);
        Assert.Equal("Bruschetta", recuperado!.Nombre);
        Assert.NotNull(recuperado.Categoria);
        Assert.Equal("Entradas", recuperado.Categoria.Nombre);
    }

    [Fact]
    public async Task ProductoRepositorio_ObtenerPorCategoria_DebeFiltrar()
    {
        var bebidas = new CategoriaProducto("Bebidas");
        var entradas = new CategoriaProducto("Entradas");
        await _uot.Categorias.AgregarAsync(bebidas);
        await _uot.Categorias.AgregarAsync(entradas);
        await _uot.GuardarCambiosAsync();

        await _uot.Productos.AgregarAsync(new Producto("Café", 3.50m, bebidas));
        await _uot.Productos.AgregarAsync(new Producto("Té", 2.50m, bebidas));
        await _uot.Productos.AgregarAsync(new Producto("Bruschetta", 8.00m, entradas));
        await _uot.GuardarCambiosAsync();

        var productosBebidas = await _uot.Productos.ObtenerPorCategoriaAsync(bebidas.Id);

        Assert.Equal(2, productosBebidas.Count);
        Assert.All(productosBebidas, p => Assert.Equal("Bebidas", p.Categoria.Nombre));
    }

    [Fact]
    public async Task ProductoRepositorio_ObtenerTodos_DebeIncluirCategoria()
    {
        var categoria = new CategoriaProducto("Postres");
        await _uot.Categorias.AgregarAsync(categoria);
        await _uot.GuardarCambiosAsync();

        await _uot.Productos.AgregarAsync(new Producto("Tiramisú", 7.00m, categoria));
        await _uot.GuardarCambiosAsync();

        var productos = await _uot.Productos.ObtenerTodosAsync();

        Assert.Single(productos);
        Assert.Equal("Postres", productos[0].Categoria.Nombre);
    }

    // --- Mesa ---

    [Fact]
    public async Task MesaRepositorio_AgregarYObtener_DebeFuncionar()
    {
        var mesa = new Mesa(3, 6);
        await _uot.Mesas.AgregarAsync(mesa);
        await _uot.GuardarCambiosAsync();

        var recuperada = await _uot.Mesas.ObtenerPorIdAsync(mesa.Id);

        Assert.NotNull(recuperada);
        Assert.Equal(3, recuperada!.Numero);
        Assert.Equal(EstadoMesa.Disponible, recuperada.Estado);
    }

    [Fact]
    public async Task MesaRepositorio_ObtenerPorNumero_DebeEncontrar()
    {
        await _uot.Mesas.AgregarAsync(new Mesa(5, 4));
        await _uot.Mesas.AgregarAsync(new Mesa(10, 2));
        await _uot.GuardarCambiosAsync();

        var mesa5 = await _uot.Mesas.ObtenerPorNumeroAsync(5);
        var mesa10 = await _uot.Mesas.ObtenerPorNumeroAsync(10);
        var inexistente = await _uot.Mesas.ObtenerPorNumeroAsync(99);

        Assert.NotNull(mesa5);
        Assert.Equal(5, mesa5!.Numero);
        Assert.NotNull(mesa10);
        Assert.Equal(10, mesa10!.Numero);
        Assert.Null(inexistente);
    }

    [Fact]
    [Trait("Category", "Regression")]
    public async Task MesaRepositorio_ActualizarEstado_DebePersistir()
    {
        var mesa = new Mesa(1, 4);
        await _uot.Mesas.AgregarAsync(mesa);
        await _uot.GuardarCambiosAsync();

        mesa.CambiarEstado(EstadoMesa.Ocupada);
        await _uot.GuardarCambiosAsync();

        var recuperada = await _uot.Mesas.ObtenerPorNumeroAsync(1);
        Assert.NotNull(recuperada);
        Assert.Equal(EstadoMesa.Ocupada, recuperada!.Estado);
    }

    // --- Pedido ---

    [Fact]
    [Trait("Category", "Regression")]
    public async Task PedidoRepositorio_AgregarYObtenerConDetalles_DebeFuncionar()
    {
        var mesa = new Mesa(2, 4);
        var categoria = new CategoriaProducto("Bebidas");
        var producto = new Producto("Café", 3.50m, categoria);

        await _uot.Mesas.AgregarAsync(mesa);
        await _uot.Categorias.AgregarAsync(categoria);
        await _uot.Productos.AgregarAsync(producto);
        await _uot.GuardarCambiosAsync();

        var pedido = new Pedido(TipoServicio.ComerAqui, mesa);
        pedido.AgregarDetalle(new DetallePedido(producto, 2, 3.50m));
        await _uot.Pedidos.AgregarAsync(pedido);
        await _uot.GuardarCambiosAsync();

        var recuperado = await _uot.Pedidos.ObtenerConDetallesAsync(pedido.Id);

        Assert.NotNull(recuperado);
        Assert.Equal(EstadoPedido.Pendiente, recuperado!.Estado);
        Assert.NotNull(recuperado.Mesa);
        Assert.Equal(2, recuperado.Mesa.Numero);
        Assert.Single(recuperado.Detalles);
        Assert.Equal(7.00m, recuperado.Total);
    }

    [Fact]
    [Trait("Category", "Regression")]
    public async Task PedidoRepositorio_ObtenerTodos_DebeRetornarLista()
    {
        var mesa = new Mesa(3, 4);
        await _uot.Mesas.AgregarAsync(mesa);
        await _uot.GuardarCambiosAsync();

        await _uot.Pedidos.AgregarAsync(new Pedido(TipoServicio.ComerAqui, mesa));
        await _uot.Pedidos.AgregarAsync(new Pedido(TipoServicio.ComerAqui, mesa));
        await _uot.GuardarCambiosAsync();

        var pedidos = await _uot.Pedidos.ObtenerTodosAsync();

        Assert.Equal(2, pedidos.Count);
        Assert.All(pedidos, p => Assert.NotNull(p.Mesa));
    }

    [Fact]
    public async Task UnidadDeTrabajo_GuardarCambios_DebePersistirMultiplesEntidades()
    {
        var mesa = new Mesa(7, 4);
        var categoria = new CategoriaProducto("Carnes");
        var producto = new Producto("Filete", 25.00m, categoria);

        await _uot.Mesas.AgregarAsync(mesa);
        await _uot.Categorias.AgregarAsync(categoria);
        await _uot.Productos.AgregarAsync(producto);
        await _uot.GuardarCambiosAsync();

        var mesaRecuperada = await _uot.Mesas.ObtenerPorNumeroAsync(7);
        var categoriaRecuperada = await _uot.Categorias.ObtenerPorIdAsync(categoria.Id);
        var productoRecuperado = await _uot.Productos.ObtenerPorIdAsync(producto.Id);

        Assert.NotNull(mesaRecuperada);
        Assert.NotNull(categoriaRecuperada);
        Assert.NotNull(productoRecuperado);
    }
}
