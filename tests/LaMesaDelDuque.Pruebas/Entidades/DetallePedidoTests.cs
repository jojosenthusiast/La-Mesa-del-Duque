using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Pruebas.Entidades;

public class DetallePedidoTests
{
    private readonly CategoriaProducto _categoria = new("Bebidas");
    private readonly Producto _producto;

    public DetallePedidoTests()
    {
        _producto = new Producto("Café Americano", 3.50m, _categoria);
    }

    [Fact]
    public void CrearDetalle_CuandoDatosSonValidos_DebeCrearInstancia()
    {
        var detalle = new DetallePedido(_producto, 2, 3.50m);

        Assert.Equal(_producto, detalle.Producto);
        Assert.Equal(2, detalle.Cantidad);
        Assert.Equal(3.50m, detalle.PrecioUnitario);
        Assert.Equal(7.00m, detalle.Subtotal);
    }

    [Fact]
    public void CalcularSubtotal_DebeRetornarCantidadPorPrecioUnitario()
    {
        var detalle = new DetallePedido(_producto, 3, 5.00m);

        Assert.Equal(15.00m, detalle.Subtotal);
    }

    [Fact]
    public void CrearDetalle_CuandoProductoEsNulo_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(
            () => new DetallePedido(null!, 1, 10m));

        Assert.Contains("producto", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearDetalle_CuandoCantidadEsCero_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(
            () => new DetallePedido(_producto, 0, 10m));

        Assert.Contains("cantidad", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearDetalle_CuandoCantidadEsNegativa_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(
            () => new DetallePedido(_producto, -1, 10m));

        Assert.Contains("cantidad", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearDetalle_CuandoPrecioUnitarioEsNegativo_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(
            () => new DetallePedido(_producto, 1, -0.01m));

        Assert.Contains("precio", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearDetalle_ConSubtotalCantidadGrande_DebeCalcularCorrectamente()
    {
        var detalle = new DetallePedido(_producto, 100, 0.50m);

        Assert.Equal(50.00m, detalle.Subtotal);
    }

    [Fact]
    public void CrearDetalle_CuandoPrecioUnitarioEsCero_DebeAceptarlo()
    {
        var detalle = new DetallePedido(_producto, 1, 0m);

        Assert.Equal(0m, detalle.Subtotal);
    }

    [Fact]
    public void ActualizarCantidad_CuandoCantidadEsValida_DebeActualizarYRecalcularSubtotal()
    {
        var detalle = new DetallePedido(_producto, 2, 3.50m);

        detalle.ActualizarCantidad(5);

        Assert.Equal(5, detalle.Cantidad);
        Assert.Equal(17.50m, detalle.Subtotal);
    }

    [Fact]
    public void ActualizarCantidad_CuandoCantidadEsCero_DebeLanzarExcepcion()
    {
        var detalle = new DetallePedido(_producto, 2, 3.50m);

        var ex = Assert.Throws<ReglaDominioException>(() => detalle.ActualizarCantidad(0));

        Assert.Contains("cantidad", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(2, detalle.Cantidad); // No debe cambiar
    }

    [Fact]
    public void ActualizarCantidad_CuandoCantidadEsNegativa_DebeLanzarExcepcion()
    {
        var detalle = new DetallePedido(_producto, 2, 3.50m);

        var ex = Assert.Throws<ReglaDominioException>(() => detalle.ActualizarCantidad(-1));

        Assert.Contains("cantidad", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(2, detalle.Cantidad); // No debe cambiar
    }
}
