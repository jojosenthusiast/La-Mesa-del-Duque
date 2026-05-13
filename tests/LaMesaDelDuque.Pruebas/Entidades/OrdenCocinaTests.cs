using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Pruebas.Entidades;

public class OrdenCocinaTests
{
    [Fact]
    public void CrearOrdenCocina_DatosValidos_DebeCrearInstancia()
    {
        var orden = new OrdenCocina(Guid.NewGuid(), Guid.NewGuid(), "Solomillo", 2, EstacionCocina.Parrilla, 5, "ComerAqui");

        Assert.NotEqual(Guid.Empty, orden.Id);
        Assert.Equal("Solomillo", orden.ProductoNombre);
        Assert.Equal(2, orden.Cantidad);
        Assert.Equal(EstacionCocina.Parrilla, orden.Estacion);
        Assert.Equal(EstadoLineaCocina.Pendiente, orden.Estado);
        Assert.True(orden.HoraRecibido > DateTime.MinValue);
        Assert.Equal(5, orden.MesaNumero);
        Assert.Equal("ComerAqui", orden.TipoServicio);
        Assert.Null(orden.HoraListo);
    }

    [Fact]
    public void CrearOrdenCocina_ParaLlevar_SinMesa_DebeCrearInstancia()
    {
        var orden = new OrdenCocina(Guid.NewGuid(), null, "Bruschetta", 1, EstacionCocina.Fria, null, "ParaLlevar");

        Assert.Null(orden.MesaNumero);
        Assert.Equal("ParaLlevar", orden.TipoServicio);
    }

    [Fact]
    public void CrearOrdenCocina_NombreVacio_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() => new OrdenCocina(Guid.NewGuid(), null, "", 1, EstacionCocina.Expo, null, null));

        Assert.Contains("nombre", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearOrdenCocina_CantidadCero_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() => new OrdenCocina(Guid.NewGuid(), null, "Café", 0, EstacionCocina.Bar, null, null));

        Assert.Contains("cantidad", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MarcarEnPreparacion_DesdePendiente_DebeCambiarEstado()
    {
        var orden = new OrdenCocina(Guid.NewGuid(), null, "Sopa", 1, EstacionCocina.Caliente, 1, "ComerAqui");

        orden.MarcarEnPreparacion();

        Assert.Equal(EstadoLineaCocina.EnPreparacion, orden.Estado);
    }

    [Fact]
    public void MarcarEnPreparacion_DesdeListo_DebeLanzarExcepcion()
    {
        var orden = new OrdenCocina(Guid.NewGuid(), null, "Sopa", 1, EstacionCocina.Caliente, 1, "ComerAqui");
        orden.MarcarComoListo();

        var ex = Assert.Throws<ReglaDominioException>(() => orden.MarcarEnPreparacion());

        Assert.Contains("pendiente", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MarcarComoListo_DesdePendiente_DebeCambiarEstadoYHoraListo()
    {
        var orden = new OrdenCocina(Guid.NewGuid(), null, "Sopa", 1, EstacionCocina.Caliente, 1, "ComerAqui");

        orden.MarcarComoListo();

        Assert.Equal(EstadoLineaCocina.Listo, orden.Estado);
        Assert.NotNull(orden.HoraListo);
    }

    [Fact]
    public void MarcarComoListo_DosVeces_DebeLanzarExcepcion()
    {
        var orden = new OrdenCocina(Guid.NewGuid(), null, "Sopa", 1, EstacionCocina.Caliente, 1, "ComerAqui");
        orden.MarcarComoListo();

        var ex = Assert.Throws<ReglaDominioException>(() => orden.MarcarComoListo());

        Assert.Contains("listo", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Recuperar_DesdeListo_DebeVolverAEnPreparacion()
    {
        var orden = new OrdenCocina(Guid.NewGuid(), null, "Sopa", 1, EstacionCocina.Caliente, 1, "ComerAqui");
        orden.MarcarComoListo();

        orden.Recuperar();

        Assert.Equal(EstadoLineaCocina.EnPreparacion, orden.Estado);
        Assert.Null(orden.HoraListo);
    }

    [Fact]
    public void Recuperar_DesdePendiente_DebeLanzarExcepcion()
    {
        var orden = new OrdenCocina(Guid.NewGuid(), null, "Sopa", 1, EstacionCocina.Caliente, 1, "ComerAqui");

        var ex = Assert.Throws<ReglaDominioException>(() => orden.Recuperar());

        Assert.Contains("recuperar", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
