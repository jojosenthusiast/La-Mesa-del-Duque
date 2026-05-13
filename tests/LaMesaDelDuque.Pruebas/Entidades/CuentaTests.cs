using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Pruebas.Entidades;

public class CuentaTests
{
    [Fact]
    public void CrearCuenta_ConDatosValidos_DebeCrearInstancia()
    {
        var pedidoId = Guid.NewGuid();

        var cuenta = new Cuenta(pedidoId, 1, 25.50m);

        Assert.NotEqual(Guid.Empty, cuenta.Id);
        Assert.Equal(pedidoId, cuenta.PedidoId);
        Assert.Equal(1, cuenta.Numero);
        Assert.Equal(25.50m, cuenta.Total);
        Assert.Equal(EstadoCuenta.Abierta, cuenta.Estado);
        Assert.Null(cuenta.MetodoPago);
        Assert.Null(cuenta.FechaPago);
    }

    [Fact]
    public void CrearCuenta_NumeroMenorQueUno_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() => new Cuenta(Guid.NewGuid(), 0, 10m));

        Assert.Contains("número de cuenta", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearCuenta_TotalNegativo_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() => new Cuenta(Guid.NewGuid(), 1, -1m));

        Assert.Contains("negativo", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Pagar_CuentaAbierta_DebeMarcarComoPagada()
    {
        var cuenta = new Cuenta(Guid.NewGuid(), 1, 20m);

        cuenta.Pagar(MetodoPago.Efectivo, 2m);

        Assert.Equal(EstadoCuenta.Pagada, cuenta.Estado);
        Assert.Equal(MetodoPago.Efectivo, cuenta.MetodoPago);
        Assert.Equal(2m, cuenta.PropinaMonto);
        Assert.NotNull(cuenta.FechaPago);
    }

    [Fact]
    public void Pagar_DosVeces_DebeLanzarExcepcion()
    {
        var cuenta = new Cuenta(Guid.NewGuid(), 1, 20m);
        cuenta.Pagar(MetodoPago.Tarjeta);

        var ex = Assert.Throws<ReglaDominioException>(() => cuenta.Pagar(MetodoPago.QR));

        Assert.Contains("ya fue pagada", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Pagar_PropinaNegativa_DebeLanzarExcepcion()
    {
        var cuenta = new Cuenta(Guid.NewGuid(), 1, 20m);

        var ex = Assert.Throws<ReglaDominioException>(() => cuenta.Pagar(MetodoPago.Transferencia, -1m));

        Assert.Contains("propina", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
