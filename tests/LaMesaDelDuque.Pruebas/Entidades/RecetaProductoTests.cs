using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Pruebas.Entidades;

public class RecetaProductoTests
{
    private readonly CategoriaProducto _categoria = new("Platos fuertes");

    [Fact]
    public void CrearReceta_ConIngredientesEInstrucciones_DebeCrearInstancia()
    {
        var producto = new Producto("Hamburguesa clásica", 6.99m, _categoria, tiempoPreparacionMin: 12);
        var pan = new Ingrediente("Pan brioche", "unidad", 20, 5, 0.35m);
        var carne = new Ingrediente("Carne 120g", "unidad", 20, 5, 1.25m);
        var ingredientes = new List<RecetaIngrediente>
        {
            new(pan, 1),
            new(carne, 1)
        };

        var receta = new RecetaProducto(producto, "Armar, sellar y servir caliente.", ingredientes);

        Assert.Equal(producto.Id, receta.ProductoId);
        Assert.Equal("Armar, sellar y servir caliente.", receta.Instrucciones);
        Assert.Equal(2, receta.Ingredientes.Count);
    }

    [Fact]
    public void CrearReceta_SinIngredientes_DebeLanzarExcepcion()
    {
        var producto = new Producto("Hamburguesa clásica", 6.99m, _categoria, tiempoPreparacionMin: 12);

        var ex = Assert.Throws<ReglaDominioException>(() =>
            new RecetaProducto(producto, "Cocinar y servir.", []));

        Assert.Contains("ingrediente", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AgregarIngrediente_Duplicado_DebeLanzarExcepcion()
    {
        var producto = new Producto("Hamburguesa clásica", 6.99m, _categoria, tiempoPreparacionMin: 12);
        var pan = new Ingrediente("Pan brioche", "unidad", 20, 5, 0.35m);
        var receta = new RecetaProducto(producto, "Cocinar y servir.", [new RecetaIngrediente(pan, 1)]);

        var ex = Assert.Throws<ReglaDominioException>(() => receta.AgregarIngrediente(new RecetaIngrediente(pan, 2)));

        Assert.Contains("ya existe", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
