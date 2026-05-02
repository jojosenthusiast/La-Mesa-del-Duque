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
}
