using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;

namespace LaMesaDelDuque.Pruebas.Dominio;

public sealed class PagoTests
{
    [Fact]
    public void CrearPago_UsuarioVacio_DebeLanzarExcepcion()
    {
        var excepcion = Assert.Throws<ArgumentException>(() =>
            new Pago(
                cuentaId: Guid.NewGuid(),
                monto: 1000m,
                metodo: MetodoPago.Efectivo,
                propinaMonto: 0,
                usuarioId: Guid.Empty));

        Assert.Contains("usuario", excepcion.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearPago_UsuarioValido_DebeCrearCorrectamente()
    {
        var pago = new Pago(
            cuentaId: Guid.NewGuid(),
            monto: 1000m,
            metodo: MetodoPago.Efectivo,
            propinaMonto: 0,
            usuarioId: Guid.NewGuid());

        Assert.NotEqual(Guid.Empty, pago.Id);
        Assert.Equal(1000m, pago.Monto);
    }
}
