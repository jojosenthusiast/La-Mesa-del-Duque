using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Pruebas.Entidades;

public class ProductoIngredienteTests
{
    [Fact]
    public void CrearProductoIngrediente_CuandoDatosSonValidos_DebeCrearRelacion()
    {
        var categoria = new CategoriaProducto("Recetas");
        var producto = new Producto("Hamburguesa", 6.50m, categoria);
        var ingrediente = new Ingrediente("Pan", "unidad", 30m, 5m, 0.2m);

        var receta = new ProductoIngrediente(producto, ingrediente, 1m);

        Assert.Equal(producto.Id, receta.ProductoId);
        Assert.Equal(ingrediente.Id, receta.IngredienteId);
        Assert.Equal(1m, receta.CantidadRequerida);
    }

    [Fact]
    public void CrearProductoIngrediente_CuandoCantidadNoEsPositiva_DebeLanzarExcepcion()
    {
        var categoria = new CategoriaProducto("Recetas");
        var producto = new Producto("Hamburguesa", 6.50m, categoria);
        var ingrediente = new Ingrediente("Pan", "unidad", 30m, 5m, 0.2m);

        var ex = Assert.Throws<ReglaDominioException>(() => new ProductoIngrediente(producto, ingrediente, 0m));

        Assert.Contains("cantidad", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
