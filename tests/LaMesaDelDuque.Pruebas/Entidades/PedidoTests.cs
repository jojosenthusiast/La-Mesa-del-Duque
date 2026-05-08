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

    private Pedido CrearPedidoConDetalle()
    {
        var pedido = new Pedido(_mesa);
        pedido.AgregarDetalle(new DetallePedido(_producto, 1, 3.50m));
        return pedido;
    }

    // --- Creación ---

    [Fact]
    public void CrearPedido_CuandoMesaEsValida_DebeCrearInstancia()
    {
        var pedido = new Pedido(_mesa);

        Assert.Equal(_mesa, pedido.Mesa);
        Assert.Equal(EstadoPedido.Pendiente, pedido.Estado);
        Assert.Empty(pedido.Detalles);
        Assert.Equal(0m, pedido.Total);
    }

    [Fact]
    public void CrearPedido_CuandoMesaEsNula_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() => new Pedido(null!));

        Assert.Contains("mesa", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearPedido_DebeIniciarComoPendiente()
    {
        var pedido = new Pedido(_mesa);

        Assert.Equal(EstadoPedido.Pendiente, pedido.Estado);
    }

    [Fact]
    public void CrearPedido_DebeIniciarSinDetalles()
    {
        var pedido = new Pedido(_mesa);

        Assert.Empty(pedido.Detalles);
    }

    // --- AgregarDetalle ---

    [Fact]
    public void AgregarDetalle_CuandoDetalleEsValido_DebeAgregarlo()
    {
        var pedido = new Pedido(_mesa);
        var detalle = new DetallePedido(_producto, 2, 3.50m);

        pedido.AgregarDetalle(detalle);

        Assert.Single(pedido.Detalles);
        Assert.Contains(detalle, pedido.Detalles);
    }

    [Fact]
    public void AgregarDetalle_CuandoDetalleEsNulo_DebeLanzarExcepcion()
    {
        var pedido = new Pedido(_mesa);

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.AgregarDetalle(null!));

        Assert.Contains("detalle", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AgregarDetalle_EnPreparacion_DebePermitirAgregar()
    {
        var pedido = CrearPedidoConDetalle();
        pedido.MarcarEnPreparacion();

        var otroDetalle = new DetallePedido(_producto, 1, 5.00m);
        pedido.AgregarDetalle(otroDetalle);

        Assert.Equal(2, pedido.Detalles.Count);
    }

    [Fact]
    public void AgregarDetalle_CuandoPedidoEstaPagado_DebeLanzarExcepcion()
    {
        var pedido = CrearPedidoConDetalle();
        pedido.MarcarComoPagado();

        var detalle = new DetallePedido(_producto, 1, 5m);
        var ex = Assert.Throws<ReglaDominioException>(() => pedido.AgregarDetalle(detalle));

        Assert.Contains("pagado", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AgregarDetalle_CuandoPedidoEstaCancelado_DebeLanzarExcepcion()
    {
        var pedido = new Pedido(_mesa);
        pedido.Cancelar();

        var detalle = new DetallePedido(_producto, 1, 5m);
        var ex = Assert.Throws<ReglaDominioException>(() => pedido.AgregarDetalle(detalle));

        Assert.Contains("cancelado", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // --- Total ---

    [Fact]
    public void Total_DebeSerSumaDeSubtotales()
    {
        var pedido = new Pedido(_mesa);
        pedido.AgregarDetalle(new DetallePedido(_producto, 2, 3.50m));
        pedido.AgregarDetalle(new DetallePedido(_producto, 1, 5.00m));

        Assert.Equal(12.00m, pedido.Total);
    }

    [Fact]
    public void Total_ConVariosProductos_DebeCalcularCorrectamente()
    {
        var pedido = new Pedido(_mesa);
        var categoria = new CategoriaProducto("Entradas");
        var otroProducto = new Producto("Bruschetta", 8.00m, categoria);
        pedido.AgregarDetalle(new DetallePedido(_producto, 3, 3.50m));
        pedido.AgregarDetalle(new DetallePedido(otroProducto, 2, 8.00m));

        Assert.Equal(26.50m, pedido.Total);
    }

    // --- MarcarEnPreparacion ---

    [Fact]
    public void MarcarEnPreparacion_DesdePendiente_DebeCambiarEstado()
    {
        var pedido = CrearPedidoConDetalle();

        pedido.MarcarEnPreparacion();

        Assert.Equal(EstadoPedido.EnPreparacion, pedido.Estado);
    }

    [Fact]
    public void MarcarEnPreparacion_SinDetalles_DebeLanzarExcepcion()
    {
        var pedido = new Pedido(_mesa);

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.MarcarEnPreparacion());

        Assert.Contains("detalle", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MarcarEnPreparacion_DesdeEnPreparacion_DebeLanzarExcepcion()
    {
        var pedido = CrearPedidoConDetalle();
        pedido.MarcarEnPreparacion();

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.MarcarEnPreparacion());

        Assert.Contains("pendiente", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MarcarEnPreparacion_DesdePagado_DebeLanzarExcepcion()
    {
        var pedido = CrearPedidoConDetalle();
        pedido.MarcarComoPagado();

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.MarcarEnPreparacion());

        Assert.Contains("pendiente", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // --- MarcarComoPagado ---

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
    public void MarcarComoPagado_SinDetalles_DebeLanzarExcepcion()
    {
        var pedido = new Pedido(_mesa);

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.MarcarComoPagado());

        Assert.Contains("detalle", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MarcarComoPagado_CuandoYaEstaPagado_DebeLanzarExcepcion()
    {
        var pedido = CrearPedidoConDetalle();
        pedido.MarcarComoPagado();

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.MarcarComoPagado());

        Assert.Contains("pagado", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MarcarComoPagado_DesdeCancelado_DebeLanzarExcepcion()
    {
        var pedido = new Pedido(_mesa);
        pedido.Cancelar();

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.MarcarComoPagado());

        Assert.Contains("cancelado", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // --- Cancelar ---

    [Fact]
    public void Cancelar_CuandoPedidoPendiente_DebeCambiarEstadoACancelado()
    {
        var pedido = new Pedido(_mesa);

        pedido.Cancelar();

        Assert.Equal(EstadoPedido.Cancelado, pedido.Estado);
    }

    [Fact]
    public void Cancelar_CuandoPedidoEnPreparacion_DebeCambiarEstadoACancelado()
    {
        var pedido = CrearPedidoConDetalle();
        pedido.MarcarEnPreparacion();

        pedido.Cancelar();

        Assert.Equal(EstadoPedido.Cancelado, pedido.Estado);
    }

    [Fact]
    public void Cancelar_CuandoPedidoPagado_DebeLanzarExcepcion()
    {
        var pedido = CrearPedidoConDetalle();
        pedido.MarcarComoPagado();

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.Cancelar());

        Assert.Contains("pagado", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Cancelar_CuandoPedidoYaCancelado_DebeLanzarExcepcion()
    {
        var pedido = new Pedido(_mesa);
        pedido.Cancelar();

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.Cancelar());

        Assert.Contains("cancelado", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // --- EliminarDetalle ---

    [Fact]
    public void EliminarDetalle_CuandoHayMultiplesDetalles_DebeEliminarYRecalcularTotal()
    {
        var pedido = new Pedido(_mesa);
        var detalle1 = new DetallePedido(_producto, 2, 3.50m); // subtotal 7.00
        var detalle2 = new DetallePedido(_producto, 1, 5.00m); // subtotal 5.00
        pedido.AgregarDetalle(detalle1);
        pedido.AgregarDetalle(detalle2);

        pedido.EliminarDetalle(detalle1.Id);

        Assert.Single(pedido.Detalles);
        Assert.Contains(detalle2, pedido.Detalles);
        Assert.Equal(5.00m, pedido.Total);
    }

    [Fact]
    public void EliminarDetalle_EnPreparacion_DebePermitirEliminar()
    {
        var pedido = new Pedido(_mesa);
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
        var pedido = new Pedido(_mesa);
        var detalle = new DetallePedido(_producto, 1, 10m);
        pedido.AgregarDetalle(detalle);

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.EliminarDetalle(detalle.Id));

        Assert.Contains("cancelar", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Single(pedido.Detalles);
    }

    [Fact]
    public void EliminarDetalle_CuandoPedidoEstaPagado_DebeLanzarExcepcion()
    {
        var pedido = new Pedido(_mesa);
        var detalle1 = new DetallePedido(_producto, 2, 3.50m);
        var detalle2 = new DetallePedido(_producto, 1, 5.00m);
        pedido.AgregarDetalle(detalle1);
        pedido.AgregarDetalle(detalle2);
        pedido.MarcarComoPagado();

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.EliminarDetalle(detalle1.Id));

        Assert.Contains("pagado", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EliminarDetalle_CuandoPedidoEstaCancelado_DebeLanzarExcepcion()
    {
        var pedido = new Pedido(_mesa);
        var detalle1 = new DetallePedido(_producto, 2, 3.50m);
        var detalle2 = new DetallePedido(_producto, 1, 5.00m);
        pedido.AgregarDetalle(detalle1);
        pedido.AgregarDetalle(detalle2);
        pedido.Cancelar();

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.EliminarDetalle(detalle1.Id));

        Assert.Contains("cancelado", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EliminarDetalle_CuandoDetalleNoExiste_DebeLanzarExcepcion()
    {
        var pedido = new Pedido(_mesa);
        pedido.AgregarDetalle(new DetallePedido(_producto, 1, 10m));

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.EliminarDetalle(Guid.NewGuid()));

        Assert.Contains("detalle", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
