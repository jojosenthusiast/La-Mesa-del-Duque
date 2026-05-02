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
}
