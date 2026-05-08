using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Pruebas.Entidades;

public class PedidoTests
{
    private readonly Mesa _mesa = new(1, 4);
    private readonly CategoriaProducto _categoria = new("Bebidas");
    private readonly Producto _producto;

    public PedidoTests()
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
    public void CrearPedido_ParaLlevar_SinMesa_DebeCrearInstancia()
    {
        var pedido = new Pedido(TipoServicio.ParaLlevar);

        Assert.Null(pedido.Mesa);
        Assert.Equal(TipoServicio.ParaLlevar, pedido.TipoServicio);
        Assert.Equal(EstadoPedido.Pendiente, pedido.Estado);
        Assert.Empty(pedido.Detalles);
    }

    [Fact]
    public void CrearPedido_ComerAqui_ConMesa_DebeCrearInstancia()
    {
        var pedido = new Pedido(TipoServicio.ComerAqui, _mesa);

        Assert.Equal(_mesa, pedido.Mesa);
        Assert.Equal(TipoServicio.ComerAqui, pedido.TipoServicio);
        Assert.Equal(EstadoPedido.Pendiente, pedido.Estado);
    }

    [Fact]
    public void CrearPedido_ComerAqui_SinMesa_DebePermitirMesaOpcional()
    {
        var pedido = new Pedido(TipoServicio.ComerAqui);

        Assert.Null(pedido.Mesa);
        Assert.Equal(TipoServicio.ComerAqui, pedido.TipoServicio);
    }

    [Fact]
    public void CrearPedido_ParaLlevar_ConMesa_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() => new Pedido(TipoServicio.ParaLlevar, _mesa));

        Assert.Contains("para llevar", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AgregarDetalle_CuandoDetalleEsValido_DebeAgregarlo()
    {
        var pedido = new Pedido(TipoServicio.ParaLlevar);
        var detalle = new DetallePedido(_producto, 2, 3.50m);

        pedido.AgregarDetalle(detalle);

        Assert.Single(pedido.Detalles);
        Assert.Contains(detalle, pedido.Detalles);
    }

    [Fact]
    public void AgregarDetalle_CuandoPedidoEstaPagado_DebeLanzarExcepcion()
    {
        var pedido = CrearPedidoConDetalle(TipoServicio.ParaLlevar);
        pedido.MarcarComoPagado();

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.AgregarDetalle(new DetallePedido(_producto, 1, 5m)));

        Assert.Contains("pagado", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Total_DebeSerSumaDeSubtotales()
    {
        var pedido = new Pedido(TipoServicio.ParaLlevar);
        pedido.AgregarDetalle(new DetallePedido(_producto, 2, 3.50m));
        pedido.AgregarDetalle(new DetallePedido(_producto, 1, 5.00m));

        Assert.Equal(12.00m, pedido.Total);
    }

    [Fact]
    public void MarcarEnPreparacion_DesdePendiente_DebeCambiarEstado()
    {
        var pedido = CrearPedidoConDetalle();

        pedido.MarcarEnPreparacion();

        Assert.Equal(EstadoPedido.EnPreparacion, pedido.Estado);
    }

    [Fact]
    public void MarcarComoPagado_DesdePendiente_DebeCambiarEstado()
    {
        var pedido = CrearPedidoConDetalle();

        pedido.MarcarComoPagado();

        Assert.Equal(EstadoPedido.Pagado, pedido.Estado);
    }

    [Fact]
    public void MarcarComoPagado_DesdeEnPreparacion_DebeCambiarEstado()
    {
        var pedido = CrearPedidoConDetalle();
        pedido.MarcarEnPreparacion();

        pedido.MarcarComoPagado();

        Assert.Equal(EstadoPedido.Pagado, pedido.Estado);
    }

    [Fact]
    public void Cancelar_CuandoPedidoPagado_DebeLanzarExcepcion()
    {
        var pedido = CrearPedidoConDetalle(TipoServicio.ParaLlevar);
        pedido.MarcarComoPagado();

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.Cancelar());

        Assert.Contains("pagado", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EliminarDetalle_EnPreparacion_DebePermitirEliminar()
    {
        var pedido = new Pedido(TipoServicio.ComerAqui, _mesa);
        var detalle1 = new DetallePedido(_producto, 2, 3.50m);
        var detalle2 = new DetallePedido(_producto, 1, 5.00m);
        pedido.AgregarDetalle(detalle1);
        pedido.AgregarDetalle(detalle2);
        pedido.MarcarEnPreparacion();

        pedido.EliminarDetalle(detalle1.Id);

        Assert.Single(pedido.Detalles);
    }

    [Fact]
    public void EliminarDetalle_CuandoEsUltimoDetalle_DebeLanzarExcepcion()
    {
        var pedido = new Pedido(TipoServicio.ParaLlevar);
        var detalle = new DetallePedido(_producto, 1, 10m);
        pedido.AgregarDetalle(detalle);

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.EliminarDetalle(detalle.Id));

        Assert.Contains("cancelar", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
