using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Pruebas.Entidades;

public class RestauranteConfigTests
{
    [Fact]
    public void CrearRestauranteConfig_CuandoDatosSonValidos_DebeCrearInstancia()
    {
        var config = new RestauranteConfig(
            "La Mesa del Duque",
            "Av. Principal 123, San Salvador",
            new TimeOnly(8, 0),
            new TimeOnly(22, 0),
            20);

        Assert.Equal("La Mesa del Duque", config.Nombre);
        Assert.Equal(1, config.Id);
        Assert.Equal(20, config.CantidadMesas);
        Assert.Null(config.Telefono);
    }

    [Fact]
    public void CrearRestauranteConfig_CuandoNombreEsVacio_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() =>
            new RestauranteConfig(" ", "Dirección válida", new TimeOnly(8, 0), new TimeOnly(22, 0), 10));

        Assert.Contains("nombre", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearRestauranteConfig_CuandoCantidadMesasEsCero_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() =>
            new RestauranteConfig("Restaurante", "Dirección", new TimeOnly(8, 0), new TimeOnly(22, 0), 0));

        Assert.Contains("mesa", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearRestauranteConfig_CuandoCierreEsAnteriorAApertura_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() =>
            new RestauranteConfig("Restaurante", "Dirección", new TimeOnly(22, 0), new TimeOnly(8, 0), 10));

        Assert.Contains("cierre", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
