using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Pruebas.Entidades;

public class MesaTests
{
    [Fact]
    public void CrearMesa_CuandoDatosSonValidos_DebeCrearInstancia()
    {
        var mesa = new Mesa(1, 4);

        Assert.Equal(1, mesa.Numero);
        Assert.Equal(4, mesa.Capacidad);
        Assert.Equal(EstadoMesa.Disponible, mesa.Estado);
        Assert.NotEqual(Guid.Empty, mesa.Id);
    }

    [Fact]
    public void CrearMesa_CuandoNumeroEsCero_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() => new Mesa(0, 4));

        Assert.Contains("número", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearMesa_CuandoNumeroEsNegativo_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() => new Mesa(-1, 4));

        Assert.Contains("número", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearMesa_CuandoCapacidadEsCero_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() => new Mesa(1, 0));

        Assert.Contains("capacidad", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearMesa_CuandoCapacidadEsNegativa_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() => new Mesa(1, -1));

        Assert.Contains("capacidad", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearMesa_DebeIniciarComoDisponible()
    {
        var mesa = new Mesa(5, 2);

        Assert.Equal(EstadoMesa.Disponible, mesa.Estado);
    }

    [Fact]
    public void CambiarEstado_CuandoEstadoEsValido_DebeActualizar()
    {
        var mesa = new Mesa(10, 6);

        mesa.CambiarEstado(EstadoMesa.Ocupada);

        Assert.Equal(EstadoMesa.Ocupada, mesa.Estado);
    }

    [Fact]
    public void CambiarEstado_CambiarAReservada_DebeActualizar()
    {
        var mesa = new Mesa(3, 8);

        mesa.CambiarEstado(EstadoMesa.Reservada);

        Assert.Equal(EstadoMesa.Reservada, mesa.Estado);
    }

    [Fact]
    public void CrearMesa_DebeEstarActivaPorDefecto()
    {
        var mesa = new Mesa(11, 4);

        Assert.True(mesa.Activa);
    }

    [Fact]
    public void DesactivarMesa_DebeMarcarComoInactiva()
    {
        var mesa = new Mesa(12, 4);

        mesa.Desactivar();

        Assert.False(mesa.Activa);
    }

    [Fact]
    public void ActivarMesa_DebeMarcarComoActiva()
    {
        var mesa = new Mesa(13, 4);
        mesa.Desactivar();

        mesa.Activar();

        Assert.True(mesa.Activa);
    }

    [Fact]
    public void ActualizarDatos_CuandoDatosSonValidos_DebeActualizarNumeroYCapacidad()
    {
        var mesa = new Mesa(14, 4);

        mesa.ActualizarDatos(20, 8);

        Assert.Equal(20, mesa.Numero);
        Assert.Equal(8, mesa.Capacidad);
    }

    [Fact]
    public void ActualizarDatos_CuandoNumeroEsInvalido_DebeLanzarExcepcion()
    {
        var mesa = new Mesa(15, 4);

        var ex = Assert.Throws<ReglaDominioException>(() => mesa.ActualizarDatos(0, 4));

        Assert.Contains("número", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ActualizarDatos_CuandoCapacidadEsInvalida_DebeLanzarExcepcion()
    {
        var mesa = new Mesa(16, 4);

        var ex = Assert.Throws<ReglaDominioException>(() => mesa.ActualizarDatos(16, -1));

        Assert.Contains("capacidad", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
