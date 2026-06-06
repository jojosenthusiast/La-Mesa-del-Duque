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

        var cuenta = new Cuenta(pedidoId, 1);
        cuenta.EstablecerTotalBase(25.50m);

        Assert.NotEqual(Guid.Empty, cuenta.Id);
        Assert.Equal(pedidoId, cuenta.PedidoId);
        Assert.Equal(1, cuenta.Numero);
        Assert.Equal(25.50m, cuenta.Total);
        Assert.Equal(EstadoCuenta.Abierta, cuenta.Estado);
        Assert.Null(cuenta.MetodoPago);
        Assert.Null(cuenta.FechaPago);
        Assert.Empty(cuenta.DetallesAsignados);
    }

    [Fact]
    public void CrearCuenta_NumeroMenorQueUno_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() => new Cuenta(Guid.NewGuid(), 0));

        Assert.Contains("número de cuenta", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EstablecerTotalBase_TotalNegativo_DebeLanzarExcepcion()
    {
        var cuenta = new Cuenta(Guid.NewGuid(), 1);

        var ex = Assert.Throws<ReglaDominioException>(() => cuenta.EstablecerTotalBase(-1m));

        Assert.Contains("negativo", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Pagar_CuentaAbierta_DebeMarcarComoPagada()
    {
        var cuenta = new Cuenta(Guid.NewGuid(), 1);
        cuenta.EstablecerTotalBase(20m);

        cuenta.Pagar(MetodoPago.Efectivo, 2m);

        Assert.Equal(EstadoCuenta.Pagada, cuenta.Estado);
        Assert.Equal(MetodoPago.Efectivo, cuenta.MetodoPago);
        Assert.Equal(2m, cuenta.PropinaMonto);
        Assert.NotNull(cuenta.FechaPago);
    }

    [Fact]
    public void Pagar_DosVeces_DebeLanzarExcepcion()
    {
        var cuenta = new Cuenta(Guid.NewGuid(), 1);
        cuenta.EstablecerTotalBase(20m);
        cuenta.Pagar(MetodoPago.Tarjeta);

        var ex = Assert.Throws<ReglaDominioException>(() => cuenta.Pagar(MetodoPago.QR));

        Assert.Contains("ya fue pagada", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Pagar_PropinaNegativa_DebeLanzarExcepcion()
    {
        var cuenta = new Cuenta(Guid.NewGuid(), 1);
        cuenta.EstablecerTotalBase(20m);

        var ex = Assert.Throws<ReglaDominioException>(() => cuenta.Pagar(MetodoPago.Transferencia, -1m));

        Assert.Contains("propina", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AsignarItem_ConDetalleValido_DebeAgregarDetalle()
    {
        var categoria = new CategoriaProducto("Bebidas");
        var producto = new Producto("Cerveza", 2.50m, categoria);
        var detalle = new DetallePedido(producto, 4, 2.50m);
        var cuenta = new Cuenta(Guid.NewGuid(), 1);

        cuenta.AsignarItem(detalle, 2);

        Assert.Single(cuenta.DetallesAsignados);
        Assert.Equal(2, cuenta.DetallesAsignados[0].CantidadAsignada);
        Assert.Equal(5.00m, cuenta.Total);
    }

    [Fact]
    public void AsignarItem_CantidadMayorQueDisponible_DebeLanzarExcepcion()
    {
        var categoria = new CategoriaProducto("Bebidas");
        var producto = new Producto("Cerveza", 2.50m, categoria);
        var detalle = new DetallePedido(producto, 2, 2.50m);
        var cuenta = new Cuenta(Guid.NewGuid(), 1);

        var ex = Assert.Throws<ReglaDominioException>(() => cuenta.AsignarItem(detalle, 3));

        Assert.Contains("exceder", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AsignarItem_SumaCantidadesExcedeTotal_DebeLanzarExcepcion()
    {
        var categoria = new CategoriaProducto("Bebidas");
        var producto = new Producto("Cerveza", 2.50m, categoria);
        var detalle = new DetallePedido(producto, 3, 2.50m);
        var cuenta = new Cuenta(Guid.NewGuid(), 1);

        cuenta.AsignarItem(detalle, 2);

        var ex = Assert.Throws<ReglaDominioException>(() => cuenta.AsignarItem(detalle, 2));

        Assert.Contains("exceder", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Total_ConItemsAsignados_DebeIgnorarTotalBase()
    {
        var categoria = new CategoriaProducto("Bebidas");
        var producto = new Producto("Cerveza", 5.00m, categoria);
        var detalle = new DetallePedido(producto, 2, 5.00m);
        var cuenta = new Cuenta(Guid.NewGuid(), 1);
        cuenta.EstablecerTotalBase(100m);

        cuenta.AsignarItem(detalle, 1);

        Assert.Equal(5.00m, cuenta.Total);
    }

    [Fact]
    public void Total_SinItemsAsignados_DebeUsarTotalBase()
    {
        var cuenta = new Cuenta(Guid.NewGuid(), 1);
        cuenta.EstablecerTotalBase(33.33m);

        Assert.Equal(33.33m, cuenta.Total);
    }
}
