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

    [Fact]
    public void CrearPedido_CuandoMesaEsValida_DebeCrearInstancia()
    {
        var pedido = new Pedido(_mesa);

        Assert.Equal(_mesa, pedido.Mesa);
        Assert.Equal(EstadoPedido.Abierto, pedido.Estado);
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
    public void CrearPedido_DebeIniciarComoAbierto()
    {
        var pedido = new Pedido(_mesa);

        Assert.Equal(EstadoPedido.Abierto, pedido.Estado);
    }

    [Fact]
    public void CrearPedido_DebeIniciarSinDetalles()
    {
        var pedido = new Pedido(_mesa);

        Assert.Empty(pedido.Detalles);
    }

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

    [Fact]
    public void Cerrar_CuandoTieneDetalles_DebeCambiarEstado()
    {
        var pedido = new Pedido(_mesa);
        pedido.AgregarDetalle(new DetallePedido(_producto, 1, 10m));

        pedido.Cerrar();

        Assert.Equal(EstadoPedido.Cerrado, pedido.Estado);
    }

    [Fact]
    public void Cerrar_CuandoNoTieneDetalles_DebeLanzarExcepcion()
    {
        var pedido = new Pedido(_mesa);

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.Cerrar());

        Assert.Contains("detalle", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Cerrar_CuandoYaEstaCerrado_DebeLanzarExcepcion()
    {
        var pedido = new Pedido(_mesa);
        pedido.AgregarDetalle(new DetallePedido(_producto, 1, 10m));
        pedido.Cerrar();

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.Cerrar());

        Assert.Contains("cerrado", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AgregarDetalle_CuandoPedidoEstaCerrado_DebeLanzarExcepcion()
    {
        var pedido = new Pedido(_mesa);
        pedido.AgregarDetalle(new DetallePedido(_producto, 1, 10m));
        pedido.Cerrar();

        var detalle = new DetallePedido(_producto, 1, 5m);
        var ex = Assert.Throws<ReglaDominioException>(() => pedido.AgregarDetalle(detalle));

        Assert.Contains("cerrado", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Cancelar_CuandoPedidoAbierto_DebeCambiarEstadoACancelado()
    {
        var pedido = new Pedido(_mesa);

        pedido.Cancelar();

        Assert.Equal(EstadoPedido.Cancelado, pedido.Estado);
    }

    [Fact]
    public void Cancelar_CuandoPedidoCerrado_DebeLanzarExcepcion()
    {
        var pedido = new Pedido(_mesa);
        pedido.AgregarDetalle(new DetallePedido(_producto, 1, 10m));
        pedido.Cerrar();

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.Cancelar());

        Assert.Contains("cerrado", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Cancelar_CuandoPedidoYaCancelado_DebeLanzarExcepcion()
    {
        var pedido = new Pedido(_mesa);
        pedido.Cancelar();

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.Cancelar());

        Assert.Contains("cancelado", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

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
    public void EliminarDetalle_CuandoPedidoEstaCerrado_DebeLanzarExcepcion()
    {
        var pedido = new Pedido(_mesa);
        var detalle1 = new DetallePedido(_producto, 2, 3.50m);
        var detalle2 = new DetallePedido(_producto, 1, 5.00m);
        pedido.AgregarDetalle(detalle1);
        pedido.AgregarDetalle(detalle2);
        pedido.Cerrar();

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.EliminarDetalle(detalle1.Id));

        Assert.Contains("cerrado", ex.Message, StringComparison.OrdinalIgnoreCase);
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
