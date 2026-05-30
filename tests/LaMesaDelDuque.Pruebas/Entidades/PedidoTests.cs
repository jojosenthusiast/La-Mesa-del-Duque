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
    public void CrearPedido_Domicilio_ConDatosEntrega_DebeCrearInstancia()
    {
        var pedido = new Pedido(
            TipoServicio.Domicilio,
            mesa: null,
            nombreClienteEntrega: "Ana Pérez",
            telefonoEntrega: "809-555-0101",
            direccionEntrega: "Calle Duarte #45, Santo Domingo",
            referenciaEntrega: "Casa azul frente al parque");

        Assert.Null(pedido.Mesa);
        Assert.Equal(TipoServicio.Domicilio, pedido.TipoServicio);
        Assert.Equal("Ana Pérez", pedido.NombreClienteEntrega);
        Assert.Equal("809-555-0101", pedido.TelefonoEntrega);
        Assert.Equal("Calle Duarte #45, Santo Domingo", pedido.DireccionEntrega);
        Assert.Equal("Casa azul frente al parque", pedido.ReferenciaEntrega);
    }

    [Fact]
    public void CrearPedido_Domicilio_ConMesa_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() => new Pedido(
            TipoServicio.Domicilio,
            _mesa,
            nombreClienteEntrega: "Ana Pérez",
            telefonoEntrega: "809-555-0101",
            direccionEntrega: "Calle Duarte #45",
            referenciaEntrega: null));

        Assert.Contains("domicilio", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("mesa", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearPedido_Domicilio_SinDireccion_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() => new Pedido(
            TipoServicio.Domicilio,
            mesa: null,
            nombreClienteEntrega: "Ana Pérez",
            telefonoEntrega: "809-555-0101",
            direccionEntrega: " ",
            referenciaEntrega: null));

        Assert.Contains("dirección", ex.Message, StringComparison.OrdinalIgnoreCase);
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

    [Fact]
    public void CrearCuentasConItems_DosCuentasConItemsDistintos_DebeCrearConTotalesCorrectos()
    {
        var pedido = new Pedido(TipoServicio.ComerAqui, _mesa);
        var productoCaro = new Producto("Filete", 25.00m, _categoria);
        var productoBarato = new Producto("Ensalada", 8.00m, _categoria);
        var detalleCaro = new DetallePedido(productoCaro, 1, 25.00m);
        var detalleBarato = new DetallePedido(productoBarato, 1, 8.00m);
        pedido.AgregarDetalle(detalleCaro);
        pedido.AgregarDetalle(detalleBarato);
        pedido.MarcarEnPreparacion();

        var asignaciones = new Dictionary<int, List<(DetallePedido detalle, int cantidad)>>
        {
            [1] = new() { (detalleCaro, 1) },
            [2] = new() { (detalleBarato, 1) }
        };

        var cuentas = pedido.CrearCuentasConItems(asignaciones);

        Assert.Equal(2, cuentas.Count);
        Assert.Equal(25.00m, cuentas[0].Total);
        Assert.Equal(8.00m, cuentas[1].Total);
    }

    [Fact]
    public void CrearCuentasConItems_CantidadParcial_DebeDividirCorrectamente()
    {
        var pedido = new Pedido(TipoServicio.ParaLlevar);
        var producto = new Producto("Cerveza", 3.00m, _categoria);
        var detalle = new DetallePedido(producto, 4, 3.00m);
        pedido.AgregarDetalle(detalle);
        pedido.MarcarEnPreparacion();

        var asignaciones = new Dictionary<int, List<(DetallePedido detalle, int cantidad)>>
        {
            [1] = new() { (detalle, 2) },
            [2] = new() { (detalle, 2) }
        };

        var cuentas = pedido.CrearCuentasConItems(asignaciones);

        Assert.Equal(6.00m, cuentas[0].Total);
        Assert.Equal(6.00m, cuentas[1].Total);
    }

    [Fact]
    public void CrearCuentasConItems_MenosDeDosCuentas_DebeLanzarExcepcion()
    {
        var pedido = CrearPedidoConDetalle();
        pedido.MarcarEnPreparacion();

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.CrearCuentasConItems(new Dictionary<int, List<(DetallePedido, int)>>()));

        Assert.Contains("al menos 2", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearCuentasConItems_CantidadExcedeDisponible_DebeLanzarExcepcion()
    {
        var pedido = new Pedido(TipoServicio.ParaLlevar);
        var detalle = new DetallePedido(_producto, 2, 3.50m);
        pedido.AgregarDetalle(detalle);
        pedido.MarcarEnPreparacion();

        var asignaciones = new Dictionary<int, List<(DetallePedido detalle, int cantidad)>>
        {
            [1] = new() { (detalle, 2) },
            [2] = new() { (detalle, 1) }
        };

        var ex = Assert.Throws<ReglaDominioException>(() => pedido.CrearCuentasConItems(asignaciones));

        Assert.Contains("excede", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearCuentas_IgualSplit_DebeFuncionarComoAntes()
    {
        var pedido = new Pedido(TipoServicio.ParaLlevar);
        pedido.AgregarDetalle(new DetallePedido(_producto, 2, 10.00m));
        pedido.MarcarEnPreparacion();

        var cuentas = pedido.CrearCuentas(2);

        Assert.Equal(2, cuentas.Count);
        Assert.Equal(10.00m, cuentas[0].Total);
        Assert.Equal(10.00m, cuentas[1].Total);
    }
}
