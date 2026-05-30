using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Pruebas.Entidades;

public class ZonaSalonTests
{
    [Fact]
    public void CrearZona_CuandoDatosSonValidos_DebeCrearInstancia()
    {
        var zona = new ZonaSalon("Terraza", 1);

        Assert.Equal("Terraza", zona.Nombre);
        Assert.Equal(1, zona.Orden);
        Assert.True(zona.Activa);
        Assert.NotEqual(Guid.Empty, zona.Id);
    }

    [Fact]
    public void CrearZona_SinOrden_DebeUsarCeroPorDefecto()
    {
        var zona = new ZonaSalon("Bar");

        Assert.Equal(0, zona.Orden);
    }

    [Fact]
    public void CrearZona_CuandoNombreEsVacio_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() => new ZonaSalon(""));

        Assert.Contains("nombre", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearZona_CuandoNombreEsNull_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() => new ZonaSalon(null!));

        Assert.Contains("nombre", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearZona_CuandoNombreEsSoloEspacios_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() => new ZonaSalon("   "));

        Assert.Contains("nombre", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearZona_CuandoNombreExcedeLongitudMaxima_DebeLanzarExcepcion()
    {
        var nombreLargo = new string('A', 101);
        var ex = Assert.Throws<ReglaDominioException>(() => new ZonaSalon(nombreLargo));

        Assert.Contains("exceder", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearZona_CuandoOrdenEsNegativo_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() => new ZonaSalon("Terraza", -1));

        Assert.Contains("orden", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ActualizarNombre_CuandoNombreEsValido_DebeActualizar()
    {
        var zona = new ZonaSalon("Terraza");

        zona.ActualizarNombre("Patio");

        Assert.Equal("Patio", zona.Nombre);
    }

    [Fact]
    public void ActualizarNombre_CuandoNombreEsVacio_DebeLanzarExcepcion()
    {
        var zona = new ZonaSalon("Terraza");

        var ex = Assert.Throws<ReglaDominioException>(() => zona.ActualizarNombre(""));

        Assert.Contains("nombre", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ActualizarNombre_CuandoNombreExcedeLongitudMaxima_DebeLanzarExcepcion()
    {
        var zona = new ZonaSalon("Terraza");
        var nombreLargo = new string('A', 101);

        var ex = Assert.Throws<ReglaDominioException>(() => zona.ActualizarNombre(nombreLargo));

        Assert.Contains("exceder", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ActualizarOrden_CuandoOrdenEsValido_DebeActualizar()
    {
        var zona = new ZonaSalon("Terraza", 0);

        zona.ActualizarOrden(2);

        Assert.Equal(2, zona.Orden);
    }

    [Fact]
    public void ActualizarOrden_CuandoOrdenEsNegativo_DebeLanzarExcepcion()
    {
        var zona = new ZonaSalon("Terraza");

        var ex = Assert.Throws<ReglaDominioException>(() => zona.ActualizarOrden(-1));

        Assert.Contains("orden", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DesactivarZona_DebeMarcarComoInactiva()
    {
        var zona = new ZonaSalon("Terraza");

        zona.Desactivar();

        Assert.False(zona.Activa);
    }

    [Fact]
    public void ActivarZona_DebeMarcarComoActiva()
    {
        var zona = new ZonaSalon("Terraza");
        zona.Desactivar();

        zona.Activar();

        Assert.True(zona.Activa);
    }

    [Fact]
    public void CrearZona_DebeRecortarEspaciosEnNombre()
    {
        var zona = new ZonaSalon("  Terraza  ");

        Assert.Equal("Terraza", zona.Nombre);
    }
}
