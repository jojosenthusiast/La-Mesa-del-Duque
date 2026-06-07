using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Pruebas.Entidades;

public class PedidoPaymentTests
{
    private readonly Mesa _mesa = new(1, 4);
    private readonly CategoriaProducto _categoria = new("Bebidas");
    private readonly Producto _producto;

    public PedidoPaymentTests()
    {
        _producto = new Producto("Café Americano", 3.50m, _categoria);
    }

    private Pedido CrearPedidoConDetalle(TipoServicio tipoServicio = TipoServicio.ComerAqui, Mesa? mesa = null)
    {
        var pedido = new Pedido(tipoServicio, mesa);
        pedido.AgregarDetalle(new DetallePedido(_producto, 1, 3.50m));
        return pedido;
    }

    [Fact]
    public void MarcarEnCobro_DesdeEnPreparacion_DebeCambiarEstado()
    {
        var pedido = CrearPedidoConDetalle();
        pedido.MarcarEnPreparacion();

        pedido.MarcarEnCobro();

        Assert.Equal(EstadoPedido.EnCobro, pedido.Estado);
    }

    [Fact]
    public void MarcarEnCobro_DesdeListo_DebeCambiarEstado()
    {
        var pedido = CrearPedidoConDetalle();
        pedido.MarcarEnPreparacion();
        pedido.MarcarListo();

        pedido.MarcarEnCobro();

        Assert.Equal(EstadoPedido.EnCobro, pedido.Estado);
    }

    [Fact]
    public void MarcarEnCobro_DesdePendiente_DebeLanzarExcepcion()
    {
        var pedido = CrearPedidoConDetalle();

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.MarcarEnCobro());

        Assert.Contains("en cobro", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Pedido_DeliveryConMesa_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() => new Pedido(TipoServicio.Delivery, _mesa));

        Assert.Contains("comer aquí", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AsignarDatosDelivery_CuandoFaltanDatos_DebeLanzarExcepcion()
    {
        var pedido = new Pedido(TipoServicio.Delivery);

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.AsignarDatosDelivery("Ana", "", "Calle 1", null, null));

        Assert.Contains("teléfono", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AsignarDatosDelivery_ConDatosValidos_DebeGuardarDatosNormalizados()
    {
        var pedido = new Pedido(TipoServicio.Delivery);

        pedido.AsignarDatosDelivery(" Ana ", " 7777-8888 ", " Calle 1 ", " Portón negro ", " Llamar al llegar ");

        Assert.Equal("Ana", pedido.ClienteDeliveryNombre);
        Assert.Equal("7777-8888", pedido.ClienteDeliveryTelefono);
        Assert.Equal("Calle 1", pedido.ClienteDeliveryDireccion);
        Assert.Equal("Portón negro", pedido.ClienteDeliveryReferencia);
        Assert.Equal("Llamar al llegar", pedido.ClienteDeliveryNotas);
    }

    [Fact]
    public void CrearCuentas_DivideTotalIgualmente()
    {
        var pedido = new Pedido(TipoServicio.ComerAqui, _mesa);
        pedido.AgregarDetalle(new DetallePedido(_producto, 2, 10m));
        pedido.AgregarDetalle(new DetallePedido(_producto, 1, 5m));
        // Total = 25m

        var cuentas = pedido.CrearCuentas(2);

        Assert.Equal(2, cuentas.Count);
        Assert.Equal(12.5m, cuentas[0].Total);
        Assert.Equal(12.5m, cuentas[1].Total);
        Assert.Equal(1, cuentas[0].Numero);
        Assert.Equal(2, cuentas[1].Numero);
    }

    [Fact]
    public void CrearCuentas_CantidadMenorQueUno_DebeLanzarExcepcion()
    {
        var pedido = CrearPedidoConDetalle();

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.CrearCuentas(0));

        Assert.Contains("al menos 1", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearCuentas_SinDetalles_DebeLanzarExcepcion()
    {
        var pedido = new Pedido(TipoServicio.ParaLlevar);

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.CrearCuentas(2));

        Assert.Contains("sin detalles", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearCuentas_ReemplazaCuentasAnteriores()
    {
        var pedido = CrearPedidoConDetalle();
        pedido.CrearCuentas(3);

        var cuentas = pedido.CrearCuentas(2);

        Assert.Equal(2, cuentas.Count);
    }

    [Fact]
    public void AgregarCuenta_CuentaDeOtroPedido_DebeLanzarExcepcion()
    {
        var pedido1 = CrearPedidoConDetalle();
        var pedido2 = CrearPedidoConDetalle();
        var cuenta = new Cuenta(pedido2.Id, 1);
        cuenta.EstablecerTotalBase(10m);

        var ex = Assert.Throws<ReglaDominioException>(() => pedido1.AgregarCuenta(cuenta));

        Assert.Contains("no pertenece", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MarcarComoPagado_SinCuentas_DebePermitir()
    {
        var pedido = CrearPedidoConDetalle();

        pedido.MarcarComoPagado();

        Assert.Equal(EstadoPedido.Pagado, pedido.Estado);
    }

    [Fact]
    public void MarcarComoPagado_ConCuentasTodasPagadas_DebePermitir()
    {
        var pedido = CrearPedidoConDetalle();
        pedido.CrearCuentas(2);
        foreach (var c in pedido.Cuentas)
            c.Pagar(MetodoPago.Efectivo);

        pedido.MarcarComoPagado();

        Assert.Equal(EstadoPedido.Pagado, pedido.Estado);
    }

    [Fact]
    public void MarcarComoPagado_ConCuentasPendientes_DebeLanzarExcepcion()
    {
        var pedido = CrearPedidoConDetalle();
        pedido.CrearCuentas(2);
        pedido.Cuentas[0].Pagar(MetodoPago.Efectivo);
        // cuenta[1] sigue abierta

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.MarcarComoPagado());

        Assert.Contains("cuentas pendientes", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EstaPagadoCompletamente_SinCuentas_DebeSerFalso()
    {
        var pedido = CrearPedidoConDetalle();

        Assert.False(pedido.EstaPagadoCompletamente);
    }

    [Fact]
    public void EstaPagadoCompletamente_TodasPagadas_DebeSerVerdadero()
    {
        var pedido = CrearPedidoConDetalle();
        pedido.CrearCuentas(2);
        pedido.Cuentas[0].Pagar(MetodoPago.Efectivo);
        pedido.Cuentas[1].Pagar(MetodoPago.Tarjeta);

        Assert.True(pedido.EstaPagadoCompletamente);
    }

    [Fact]
    public void EstaPagadoCompletamente_AlMenosUnaPendiente_DebeSerFalso()
    {
        var pedido = CrearPedidoConDetalle();
        pedido.CrearCuentas(2);
        pedido.Cuentas[0].Pagar(MetodoPago.Efectivo);

        Assert.False(pedido.EstaPagadoCompletamente);
    }
}
