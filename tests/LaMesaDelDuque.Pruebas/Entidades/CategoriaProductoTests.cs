using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Pruebas.Entidades;

public class CategoriaProductoTests
{
    [Fact]
    public void CrearCategoria_CuandoNombreEsValido_DebeCrearInstancia()
    {
        var categoria = new CategoriaProducto("Bebidas");

        Assert.Equal("Bebidas", categoria.Nombre);
        Assert.True(categoria.Activo);
    }

    [Fact]
    public void CrearCategoria_CuandoNombreEsNulo_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() => new CategoriaProducto(null!));

        Assert.Contains("nombre", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearCategoria_CuandoNombreEsVacio_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() => new CategoriaProducto(string.Empty));

        Assert.Contains("nombre", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearCategoria_CuandoNombreEsBlanco_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() => new CategoriaProducto("   "));

        Assert.Contains("nombre", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearCategoria_DebeEstarActivaPorDefecto()
    {
        var categoria = new CategoriaProducto("Entradas");

        Assert.True(categoria.Activo);
    }

    [Fact]
    public void Desactivar_CuandoCategoriaEstaActiva_DebeCambiarEstado()
    {
        var categoria = new CategoriaProducto("Postres");

        categoria.Desactivar();

        Assert.False(categoria.Activo);
    }

    [Fact]
    public void Activar_CuandoCategoriaEstaInactiva_DebeCambiarEstado()
    {
        var categoria = new CategoriaProducto("Carnes");
        categoria.Desactivar();

        categoria.Activar();

        Assert.True(categoria.Activo);
    }

    [Fact]
    public void IdDebeSerInicializado()
    {
        var categoria = new CategoriaProducto("Vinos");

        Assert.NotEqual(Guid.Empty, categoria.Id);
    }

    [Fact]
    public void CrearCategoria_CuandoIncluyeDescripcionYOrdenDisplay_DebePersistirValores()
    {
        var categoria = new CategoriaProducto("Especiales", "Categoría premium", 3);

        Assert.Equal("Categoría premium", categoria.Descripcion);
        Assert.Equal(3, categoria.OrdenDisplay);
    }

    [Fact]
    public void CrearCategoria_CuandoNombreExcedeLongitudCanonica_DebeLanzarExcepcion()
    {
        var nombreMuyLargo = new string('A', 101);

        var ex = Assert.Throws<ReglaDominioException>(() => new CategoriaProducto(nombreMuyLargo));

        Assert.Contains("100", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
