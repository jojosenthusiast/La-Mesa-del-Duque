using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Pruebas.Entidades;

public class ProductoTests
{
    private readonly CategoriaProducto _categoria = new("Bebidas");

    [Fact]
    public void CrearProducto_CuandoDatosSonValidos_DebeCrearInstancia()
    {
        var producto = new Producto("Café Americano", 3.50m, _categoria);

        Assert.Equal("Café Americano", producto.Nombre);
        Assert.Equal(3.50m, producto.Precio);
        Assert.Equal(_categoria, producto.Categoria);
        Assert.True(producto.Activo);
        Assert.NotEqual(Guid.Empty, producto.Id);
    }

    [Fact]
    public void CrearProducto_CuandoPrecioEsCero_DebeAceptarlo()
    {
        var producto = new Producto("Agua del grifo", 0m, _categoria);

        Assert.Equal(0m, producto.Precio);
    }

    [Fact]
    public void CrearProducto_CuandoNombreEsNulo_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(
            () => new Producto(null!, 10m, _categoria));

        Assert.Contains("nombre", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearProducto_CuandoNombreEsVacio_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(
            () => new Producto(string.Empty, 10m, _categoria));

        Assert.Contains("nombre", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearProducto_CuandoNombreEsBlanco_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(
            () => new Producto("   ", 10m, _categoria));

        Assert.Contains("nombre", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearProducto_CuandoPrecioEsNegativo_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(
            () => new Producto("Té helado", -1m, _categoria));

        Assert.Contains("precio", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearProducto_CuandoCategoriaEsNula_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(
            () => new Producto("Café", 2.50m, null!));

        Assert.Contains("categoría", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearProducto_DebeEstarActivoPorDefecto()
    {
        var producto = new Producto("Limonada", 4.00m, _categoria);

        Assert.True(producto.Activo);
    }

    [Fact]
    public void Desactivar_CuandoProductoEstaActivo_DebeCambiarEstado()
    {
        var producto = new Producto("Smoothie", 5.50m, _categoria);

        producto.Desactivar();

        Assert.False(producto.Activo);
    }

    [Fact]
    public void Activar_CuandoProductoEstaInactivo_DebeCambiarEstado()
    {
        var producto = new Producto("Malteada", 6.00m, _categoria);
        producto.Desactivar();

        producto.Activar();

        Assert.True(producto.Activo);
    }

    [Fact]
    public void CrearProducto_DebeTenerDescripcionNulaPorDefecto()
    {
        var producto = new Producto("Té verde", 2.00m, _categoria);

        Assert.Null(producto.Descripcion);
    }

    [Fact]
    public void ActualizarDatos_CuandoDatosSonValidos_DebeActualizarNombrePrecioYCategoria()
    {
        var producto = new Producto("Café", 3.50m, _categoria);
        var nuevaCategoria = new CategoriaProducto("Tés");

        producto.ActualizarDatos("Té Chai", 4.50m, nuevaCategoria);

        Assert.Equal("Té Chai", producto.Nombre);
        Assert.Equal(4.50m, producto.Precio);
        Assert.Equal(nuevaCategoria, producto.Categoria);
    }

    [Fact]
    public void ActualizarDatos_CuandoPrecioEsNegativo_DebeLanzarExcepcion()
    {
        var producto = new Producto("Café", 3.50m, _categoria);

        var ex = Assert.Throws<ReglaDominioException>(
            () => producto.ActualizarDatos("Café", -1m, _categoria));

        Assert.Contains("precio", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // INVARIANTE CANÓNICA (CHECK >= 0): precio cero debe ser consistente entre crear y actualizar
    [Fact]
    public void ActualizarDatos_CuandoPrecioEsCero_DebeAceptarlo()
    {
        var producto = new Producto("Café", 3.50m, _categoria);

        producto.ActualizarDatos("Café del día", 0m, _categoria);

        Assert.Equal(0m, producto.Precio);
    }

    // ActualizarImagen debe aplicar el mismo límite que el constructor (500 chars)
    [Fact]
    public void ActualizarImagen_CuandoUrlExcedeLongitudCanonica_DebeLanzarExcepcion()
    {
        var producto = new Producto("Café", 3.50m, _categoria);
        var urlMuyLarga = "https://cdn.lmd/" + new string('x', 490);

        var ex = Assert.Throws<ReglaDominioException>(
            () => producto.ActualizarImagen(urlMuyLarga));

        Assert.Contains("500", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ActualizarImagen con URL válida dentro del límite debe funcionar
    [Fact]
    public void ActualizarImagen_CuandoUrlEsValida_DebeActualizar()
    {
        var producto = new Producto("Café", 3.50m, _categoria);
        var url = "https://cdn.lmd/menu/cafe.jpg";

        producto.ActualizarImagen(url);

        Assert.Equal(url, producto.ImagenUrl);
    }

    [Fact]
    public void ActualizarDatos_CuandoNombreEsInvalido_DebeLanzarExcepcion()
    {
        var producto = new Producto("Café", 3.50m, _categoria);

        var ex = Assert.Throws<ReglaDominioException>(
            () => producto.ActualizarDatos("   ", 5m, _categoria));

        Assert.Contains("nombre", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ActualizarDatos_CuandoCategoriaEsNula_DebeLanzarExcepcion()
    {
        var producto = new Producto("Café", 3.50m, _categoria);

        var ex = Assert.Throws<ReglaDominioException>(
            () => producto.ActualizarDatos("Café", 5m, null!));

        Assert.Contains("categoría", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ActualizarDescripcion_CuandoEsTextoValido_DebeActualizar()
    {
        var producto = new Producto("Café", 3.50m, _categoria);

        producto.ActualizarDescripcion("Café orgánico de altura, tostado medio.");

        Assert.Equal("Café orgánico de altura, tostado medio.", producto.Descripcion);
    }

    [Fact]
    public void ActualizarDescripcion_CuandoEsNula_DebePermitirlo()
    {
        var producto = new Producto("Café", 3.50m, _categoria);
        producto.ActualizarDescripcion("Tiene descripción");

        producto.ActualizarDescripcion(null);

        Assert.Null(producto.Descripcion);
    }

    [Fact]
    public void CrearProducto_CuandoIncluyeImagenYTiempoPreparacion_DebePersistirValores()
    {
        var producto = new Producto("Hamburguesa clásica", 6.99m, _categoria, "https://cdn.lmd/menu/hamburguesa.jpg", 12);

        Assert.Equal("https://cdn.lmd/menu/hamburguesa.jpg", producto.ImagenUrl);
        Assert.Equal(12, producto.TiempoPreparacionMin);
    }

    [Fact]
    public void CrearProducto_CuandoNombreExcedeLongitudCanonica_DebeLanzarExcepcion()
    {
        var nombreMuyLargo = new string('P', 151);

        var ex = Assert.Throws<ReglaDominioException>(() => new Producto(nombreMuyLargo, 7.50m, _categoria));

        Assert.Contains("150", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
