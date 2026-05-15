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

public class CatalogoProductosServicioTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly LaMesaDelDuqueDbContext _contexto;
    private readonly ICatalogoProductosServicio _servicio;
    private readonly IUnidadDeTrabajo _uot;

    public CatalogoProductosServicioTests()
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
            new CuentaRepositorio(_contexto));

        _servicio = new CatalogoProductosServicio(_uot);
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
    }

    [Fact]
    public async Task ListarCategorias_SinCategorias_DebeRetornarListaVacia()
    {
        var categorias = await _servicio.ListarCategoriasAsync();

        Assert.NotNull(categorias);
        Assert.Empty(categorias);
    }

    [Fact]
    public async Task CrearCategoria_ConNombreValido_DebeCrearYRetornarDto()
    {
        var dto = await _servicio.CrearCategoriaAsync("Bebidas");

        Assert.NotNull(dto);
        Assert.NotEqual(Guid.Empty, dto.Id);
        Assert.Equal("Bebidas", dto.Nombre);
        Assert.True(dto.Activo);
    }

    [Fact]
    public async Task CrearCategoria_ConNombreVacio_DebeLanzarExcepcion()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.CrearCategoriaAsync(""));
    }

    [Fact]
    public async Task ListarCategorias_ConCategorias_DebeRetornarTodas()
    {
        await _servicio.CrearCategoriaAsync("Bebidas");
        await _servicio.CrearCategoriaAsync("Entradas");

        var categorias = await _servicio.ListarCategoriasAsync();

        Assert.Equal(2, categorias.Count);
        Assert.Contains(categorias, c => c.Nombre == "Bebidas");
        Assert.Contains(categorias, c => c.Nombre == "Entradas");
    }

    [Fact]
    public async Task ListarProductos_SinProductos_DebeRetornarListaVacia()
    {
        var productos = await _servicio.ListarProductosAsync();

        Assert.NotNull(productos);
        Assert.Empty(productos);
    }

    [Fact]
    public async Task CrearProducto_ConDatosValidos_DebeCrearYRetornarDto()
    {
        var categoria = await _servicio.CrearCategoriaAsync("Bebidas");

        var dto = await _servicio.CrearProductoAsync("Café Americano", 3.50m, categoria.Id);

        Assert.NotNull(dto);
        Assert.NotEqual(Guid.Empty, dto.Id);
        Assert.Equal("Café Americano", dto.Nombre);
        Assert.Equal(3.50m, dto.Precio);
        Assert.Equal(categoria.Id, dto.CategoriaId);
        Assert.Equal("Bebidas", dto.CategoriaNombre);
    }

    [Fact]
    public async Task CrearProducto_ConDatosCompletos_DebePersistirDescripcionImagenYTiempoPreparacion()
    {
        var categoria = await _servicio.CrearCategoriaAsync("Platos fuertes");

        var dto = await _servicio.CrearProductoAsync(
            "Hamburguesa clásica",
            6.99m,
            categoria.Id,
            "Pan brioche, carne y vegetales frescos.",
            "https://cdn.lmd/menu/hamburguesa.jpg",
            12);

        Assert.Equal("Pan brioche, carne y vegetales frescos.", dto.Descripcion);
        Assert.Equal("https://cdn.lmd/menu/hamburguesa.jpg", dto.ImagenUrl);
        Assert.Equal(12, dto.TiempoPreparacionMin);
    }

    [Fact]
    public async Task CrearProducto_PrecioCero_DebeUsarMensajeEsperado()
    {
        var categoria = await _servicio.CrearCategoriaAsync("Bebidas");

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.CrearProductoAsync("Agua filtrada", 0m, categoria.Id));

        Assert.Contains("mayor que cero", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CrearProducto_ConCategoriaInexistente_DebeLanzarExcepcion()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.CrearProductoAsync("Fake", 10m, Guid.NewGuid()));
    }

    [Fact]
    public async Task ListarProductos_ConProductos_DebeRetornarTodos()
    {
        var bebidas = await _servicio.CrearCategoriaAsync("Bebidas");
        var entradas = await _servicio.CrearCategoriaAsync("Entradas");

        await _servicio.CrearProductoAsync("Café", 3.50m, bebidas.Id);
        await _servicio.CrearProductoAsync("Té", 2.50m, bebidas.Id);
        await _servicio.CrearProductoAsync("Bruschetta", 8.00m, entradas.Id);

        var productos = await _servicio.ListarProductosAsync();

        Assert.Equal(3, productos.Count);
    }

    [Fact]
    public async Task ListarProductosPorCategoria_DebeFiltrarCorrectamente()
    {
        var bebidas = await _servicio.CrearCategoriaAsync("Bebidas");
        var entradas = await _servicio.CrearCategoriaAsync("Entradas");

        await _servicio.CrearProductoAsync("Café", 3.50m, bebidas.Id);
        await _servicio.CrearProductoAsync("Té", 2.50m, bebidas.Id);
        await _servicio.CrearProductoAsync("Bruschetta", 8.00m, entradas.Id);

        var productosBebidas = await _servicio.ListarProductosPorCategoriaAsync(bebidas.Id);

        Assert.Equal(2, productosBebidas.Count);
        Assert.All(productosBebidas, p => Assert.Equal("Bebidas", p.CategoriaNombre));
    }

    [Fact]
    public async Task ActualizarProducto_ConDatosValidos_DebeActualizarYPersistir()
    {
        var bebidas = await _servicio.CrearCategoriaAsync("Bebidas");
        var tes = await _servicio.CrearCategoriaAsync("Tés");
        var producto = await _servicio.CrearProductoAsync("Café", 3.50m, bebidas.Id);

        var actualizado = await _servicio.ActualizarProductoAsync(producto.Id, "Té Chai", 4.50m, tes.Id, "Té especiado de la India");

        Assert.Equal("Té Chai", actualizado.Nombre);
        Assert.Equal(4.50m, actualizado.Precio);
        Assert.Equal(tes.Id, actualizado.CategoriaId);
        Assert.Equal("Tés", actualizado.CategoriaNombre);
        Assert.Equal("Té especiado de la India", actualizado.Descripcion);
    }

    [Fact]
    public async Task ActualizarProducto_ConImagenYTiempoPreparacion_DebeActualizarCamposCompletos()
    {
        var bebidas = await _servicio.CrearCategoriaAsync("Bebidas especiales");
        var producto = await _servicio.CrearProductoAsync("Latte", 4.25m, bebidas.Id);

        var actualizado = await _servicio.ActualizarProductoAsync(
            producto.Id,
            "Latte vainilla",
            4.75m,
            bebidas.Id,
            "Bebida caliente con vainilla.",
            "https://cdn.lmd/menu/latte-vainilla.jpg",
            8);

        Assert.Equal("https://cdn.lmd/menu/latte-vainilla.jpg", actualizado.ImagenUrl);
        Assert.Equal(8, actualizado.TiempoPreparacionMin);
    }

    [Fact]
    public async Task ActualizarProducto_Inexistente_DebeLanzarExcepcion()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.ActualizarProductoAsync(Guid.NewGuid(), "X", 10m, Guid.NewGuid(), null));
    }

    [Fact]
    public async Task DesactivarProducto_SinPedidosActivos_DebeDesactivar()
    {
        var categoria = await _servicio.CrearCategoriaAsync("Bebidas");
        var producto = await _servicio.CrearProductoAsync("Agua Mineral", 2.00m, categoria.Id);

        await _servicio.DesactivarProductoAsync(producto.Id);

        var productos = await _servicio.ListarProductosAsync();
        var desactivado = productos.First(p => p.Id == producto.Id);
        Assert.False(desactivado.Activo);
    }

    [Fact]
    public async Task DesactivarProducto_ConPedidosActivos_DebeLanzarExcepcion()
    {
        var categoria = await _servicio.CrearCategoriaAsync("Bebidas");
        var producto = await _servicio.CrearProductoAsync("Café", 3.00m, categoria.Id);

        // Crear un pedido activo que incluya este producto usando la UoT directamente
        var mesa = new Mesa(100, 4);
        await _uot.Mesas.AgregarAsync(mesa);
        // Re-obtener producto con tracking para EF
        var productoTracked = await _uot.Productos.ObtenerConTrackingAsync(producto.Id);
        var pedido = new Pedido(TipoServicio.ComerAqui, mesa);
        pedido.AgregarDetalle(new DetallePedido(productoTracked!, 1, 3.00m));
        await _uot.Pedidos.AgregarAsync(pedido);
        await _uot.GuardarCambiosAsync();

        await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.DesactivarProductoAsync(producto.Id));
    }

    [Fact]
    public async Task DesactivarProducto_ConPedidosActivos_DebeUsarMensajeEsperado()
    {
        var categoria = await _servicio.CrearCategoriaAsync("Bebidas premium");
        var producto = await _servicio.CrearProductoAsync("Café reserva", 4.50m, categoria.Id);

        var mesa = new Mesa(101, 4);
        await _uot.Mesas.AgregarAsync(mesa);
        var productoTracked = await _uot.Productos.ObtenerConTrackingAsync(producto.Id);
        var pedido = new Pedido(TipoServicio.ComerAqui, mesa);
        pedido.AgregarDetalle(new DetallePedido(productoTracked!, 1, 4.50m));
        await _uot.Pedidos.AgregarAsync(pedido);
        await _uot.GuardarCambiosAsync();

        var ex = await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.DesactivarProductoAsync(producto.Id));

        Assert.Contains("pedidos activos", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DesactivarProducto_Inexistente_DebeLanzarExcepcion()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.DesactivarProductoAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ActualizarCategoria_ConNombreValido_DebeActualizar()
    {
        var categoria = await _servicio.CrearCategoriaAsync("Bebidas");

        var actualizada = await _servicio.ActualizarCategoriaAsync(categoria.Id, "Bebidas Premium");

        Assert.Equal("Bebidas Premium", actualizada.Nombre);
    }

    [Fact]
    public async Task ActualizarCategoria_Inexistente_DebeLanzarExcepcion()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.ActualizarCategoriaAsync(Guid.NewGuid(), "X"));
    }

    [Fact]
    public async Task DesactivarCategoria_Existente_DebeDesactivar()
    {
        var categoria = await _servicio.CrearCategoriaAsync("Temporales");

        await _servicio.DesactivarCategoriaAsync(categoria.Id);

        var categorias = await _servicio.ListarCategoriasAsync();
        var desactivada = categorias.First(c => c.Id == categoria.Id);
        Assert.False(desactivada.Activo);
    }

    [Fact]
    public async Task DesactivarCategoria_Inexistente_DebeLanzarExcepcion()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.DesactivarCategoriaAsync(Guid.NewGuid()));
    }
}
