using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Pruebas.Entidades;

public class IngredienteTests
{
    [Fact]
    public void CrearIngrediente_CuandoDatosSonValidos_DebeCrearInstancia()
    {
        var ingrediente = new Ingrediente("Pan brioche", "unidad", 30m, 10m, 0.35m);

        Assert.Equal("Pan brioche", ingrediente.Nombre);
        Assert.Equal("unidad", ingrediente.UnidadMedida);
        Assert.Equal(30m, ingrediente.StockActual);
        Assert.Equal(10m, ingrediente.StockMinimo);
        Assert.Equal(0.35m, ingrediente.CostoUnitario);
        Assert.True(ingrediente.Activo);
    }

    [Fact]
    public void CrearIngrediente_CuandoStockEsNegativo_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() =>
            new Ingrediente("Carne", "gramo", -1m, 0m, 2.5m));

        Assert.Contains("stock", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
    [Fact]
    public void DescontarStock_CuandoCantidadSuperaDisponible_DebeRechazarSinClampearACero()
    {
        var ingrediente = new Ingrediente("Queso mozzarella", "kg", 1m, 0m, 4m);

        var ex = Assert.Throws<ReglaDominioException>(() => ingrediente.DescontarStock(2m));

        Assert.Contains("stock", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1m, ingrediente.StockActual);
    }
}
