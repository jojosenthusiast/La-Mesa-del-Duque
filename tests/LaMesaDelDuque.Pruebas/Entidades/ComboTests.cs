using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Pruebas.Entidades;

public class ComboTests
{
    [Fact]
    public void CrearCombo_CuandoDatosSonValidos_DebeCrearInstancia()
    {
        var inicio = new DateOnly(2026, 1, 1);
        var combo = new Combo("Combo Familiar", 15.99m, inicio, "Descripción del combo");

        Assert.Equal("Combo Familiar", combo.Nombre);
        Assert.Equal(15.99m, combo.PrecioCombo);
        Assert.True(combo.Activo);
        Assert.NotEqual(Guid.Empty, combo.Id);
        Assert.Null(combo.FechaFin);
    }

    [Fact]
    public void CrearCombo_CuandoPrecioEsCero_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() =>
            new Combo("Combo Test", 0m, new DateOnly(2026, 1, 1)));

        Assert.Contains("precio", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearCombo_CuandoFechaFinEsAnteriorAInicio_DebeLanzarExcepcion()
    {
        var inicio = new DateOnly(2026, 6, 1);
        var fin = new DateOnly(2026, 1, 1);

        var ex = Assert.Throws<ReglaDominioException>(() =>
            new Combo("Combo Test", 10m, inicio, fechaFin: fin));

        Assert.Contains("fin", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
