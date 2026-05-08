using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using LaMesaDelDuque.Infraestructura.Repositorios;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Pruebas.Aplicacion;

public class PedidosServicioTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly LaMesaDelDuqueDbContext _contexto;
    private readonly IPedidosServicio _servicio;
    private readonly IUnidadDeTrabajo _uot;

    public PedidosServicioTests()
    {
        _conexion = new SqliteConnection("Data Source=:memory:");
        _conexion.Open();

        var opciones = new DbContextOptionsBuilder<LaMesaDelDuqueDbContext>()
            .UseSqlite(_conexion)
            .Options;

        _contexto = new LaMesaDelDuqueDbContext(opciones);
        _contexto.Database.EnsureCreated();

        _uot = new UnidadDeTrabajo(_contexto,
            new CategoriaProductoRepositorio(_contexto),
            new ProductoRepositorio(_contexto),
            new MesaRepositorio(_contexto),
            new PedidoRepositorio(_contexto));

        _servicio = new PedidosServicio(_uot);
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
    }

    // --- CrearPedido ---

    [Fact]
    [Trait("Category", "Regression")]
    public async Task CrearPedido_ConUnDetalle_DebeCrearYRetornarDtoConTotal()
    {
        var mesa = new Mesa(1, 4);
        await _uot.Mesas.AgregarAsync(mesa);

        var categoria = new CategoriaProducto("Bebidas");
        await _uot.Categorias.AgregarAsync(categoria);

        var producto = new Producto("Café", 3.50m, categoria);
        await _uot.Productos.AgregarAsync(producto);

        await _uot.GuardarCambiosAsync();

        var detalles = new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 2, PrecioUnitario = 3.50m }
        };
        var pedidoDto = await _servicio.CrearPedidoAsync(mesa.Id, detalles);

        Assert.NotNull(pedidoDto);
        Assert.NotEqual(Guid.Empty, pedidoDto.Id);
        Assert.Equal(mesa.Id, pedidoDto.MesaId);
        Assert.Equal(mesa.Numero, pedidoDto.MesaNumero);
        Assert.Equal("Pendiente", pedidoDto.Estado);
        Assert.Equal(7.00m, pedidoDto.Total);
        Assert.Single(pedidoDto.Detalles);
        Assert.Equal(producto.Id, pedidoDto.Detalles[0].ProductoId);
        Assert.Equal("Café", pedidoDto.Detalles[0].ProductoNombre);
        Assert.Equal(2, pedidoDto.Detalles[0].Cantidad);
        Assert.Equal(3.50m, pedidoDto.Detalles[0].PrecioUnitario);
        Assert.Equal(7.00m, pedidoDto.Detalles[0].Subtotal);
    }

    [Fact]
    public async Task CrearPedido_ConVariosDetalles_DebeSumarTotalCorrecto()
    {
        var mesa = new Mesa(2, 4);
        await _uot.Mesas.AgregarAsync(mesa);

        var categoria = new CategoriaProducto("Entradas");
        await _uot.Categorias.AgregarAsync(categoria);

        var p1 = new Producto("Bruschetta", 8.00m, categoria);
        var p2 = new Producto("Ensalada", 6.50m, categoria);
        await _uot.Productos.AgregarAsync(p1);
        await _uot.Productos.AgregarAsync(p2);

        await _uot.GuardarCambiosAsync();

        var detalles = new List<DetalleCreacionDto>
        {
            new() { ProductoId = p1.Id, Cantidad = 1, PrecioUnitario = 8.00m },
            new() { ProductoId = p2.Id, Cantidad = 2, PrecioUnitario = 6.50m }
        };

        var pedidoDto = await _servicio.CrearPedidoAsync(mesa.Id, detalles);

        Assert.Equal(21.00m, pedidoDto.Total);
        Assert.Equal(2, pedidoDto.Detalles.Count);
    }

    [Fact]
    public async Task CrearPedido_SinDetalles_DebeLanzarExcepcion()
    {
        var mesa = new Mesa(3, 4);
        await _uot.Mesas.AgregarAsync(mesa);
        await _uot.GuardarCambiosAsync();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.CrearPedidoAsync(mesa.Id, new List<DetalleCreacionDto>()));
    }

    [Fact]
    public async Task CrearPedido_ConMesaInexistente_DebeLanzarExcepcion()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.CrearPedidoAsync(Guid.NewGuid(), new List<DetalleCreacionDto>
            {
                new() { ProductoId = Guid.NewGuid(), Cantidad = 1, PrecioUnitario = 10m }
            }));
    }

    [Fact]
    public async Task CrearPedido_ConProductoInexistente_DebeLanzarExcepcion()
    {
        var mesa = new Mesa(4, 4);
        await _uot.Mesas.AgregarAsync(mesa);
        await _uot.GuardarCambiosAsync();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.CrearPedidoAsync(mesa.Id, new List<DetalleCreacionDto>
            {
                new() { ProductoId = Guid.NewGuid(), Cantidad = 1, PrecioUnitario = 10m }
            }));
    }

    // --- ObtenerPedido ---

    [Fact]
    public async Task ObtenerPedido_Existente_DebeIncluirDetallesProductoYMesa()
    {
        var mesa = new Mesa(5, 4);
        await _uot.Mesas.AgregarAsync(mesa);

        var categoria = new CategoriaProducto("Postres");
        await _uot.Categorias.AgregarAsync(categoria);

        var producto = new Producto("Tiramisú", 7.00m, categoria);
        await _uot.Productos.AgregarAsync(producto);

        await _uot.GuardarCambiosAsync();

        var dtoCreado = await _servicio.CrearPedidoAsync(mesa.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 3, PrecioUnitario = 7.00m }
        });

        var obtenido = await _servicio.ObtenerPedidoAsync(dtoCreado.Id);

        Assert.NotNull(obtenido);
        Assert.Equal(dtoCreado.Id, obtenido!.Id);
        Assert.Equal(mesa.Id, obtenido.MesaId);
        Assert.Equal(mesa.Numero, obtenido.MesaNumero);
        Assert.Single(obtenido.Detalles);
        Assert.Equal(producto.Id, obtenido.Detalles[0].ProductoId);
        Assert.Equal("Tiramisú", obtenido.Detalles[0].ProductoNombre);
        Assert.Equal(3, obtenido.Detalles[0].Cantidad);
        Assert.Equal(21.00m, obtenido.Total);
    }

    [Fact]
    public async Task ObtenerPedido_Inexistente_DebeRetornarNulo()
    {
        var resultado = await _servicio.ObtenerPedidoAsync(Guid.NewGuid());
        Assert.Null(resultado);
    }

    // --- ListarPedidosActivos ---

    [Fact]
    public async Task ListarPedidosActivos_DebeIncluirPendientesYEnPreparacion()
    {
        var mesa1 = new Mesa(10, 4);
        var mesa2 = new Mesa(11, 4);
        var mesa3 = new Mesa(12, 4);
        await _uot.Mesas.AgregarAsync(mesa1);
        await _uot.Mesas.AgregarAsync(mesa2);
        await _uot.Mesas.AgregarAsync(mesa3);

        var categoria = new CategoriaProducto("Bebidas");
        await _uot.Categorias.AgregarAsync(categoria);

        var producto = new Producto("Café", 3.00m, categoria);
        await _uot.Productos.AgregarAsync(producto);

        await _uot.GuardarCambiosAsync();

        var detalles = new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 3.00m }
        };

        var pendiente = await _servicio.CrearPedidoAsync(mesa1.Id, detalles);
        var enPrep = await _servicio.CrearPedidoAsync(mesa2.Id, detalles);
        var pagado = await _servicio.CrearPedidoAsync(mesa3.Id, detalles);

        await _servicio.MarcarEnPreparacionAsync(enPrep.Id);
        await _servicio.PagarPedidoAsync(pagado.Id);

        var activos = await _servicio.ListarPedidosActivosAsync();

        Assert.Equal(2, activos.Count);
        Assert.Contains(activos, a => a.Id == pendiente.Id);
        Assert.Contains(activos, a => a.Id == enPrep.Id);
    }

    [Fact]
    public async Task ListarPedidosActivos_SinPedidos_DebeRetornarListaVacia()
    {
        var activos = await _servicio.ListarPedidosActivosAsync();

        Assert.NotNull(activos);
        Assert.Empty(activos);
    }

    // --- AgregarDetalle ---

    [Fact]
    public async Task AgregarDetalle_PedidoPendiente_DebeAgregarYRecalcularTotal()
    {
        var mesa = new Mesa(20, 4);
        await _uot.Mesas.AgregarAsync(mesa);

        var categoria = new CategoriaProducto("Bebidas");
        await _uot.Categorias.AgregarAsync(categoria);

        var cafe = new Producto("Café", 3.00m, categoria);
        var te = new Producto("Té", 2.50m, categoria);
        await _uot.Productos.AgregarAsync(cafe);
        await _uot.Productos.AgregarAsync(te);

        await _uot.GuardarCambiosAsync();

        var pedido = await _servicio.CrearPedidoAsync(mesa.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = cafe.Id, Cantidad = 2, PrecioUnitario = 3.00m }
        });
        Assert.Equal(6.00m, pedido.Total);

        var actualizado = await _servicio.AgregarDetalleAsync(pedido.Id, te.Id, 1, 2.50m);

        Assert.Equal(2, actualizado.Detalles.Count);
        Assert.Equal(8.50m, actualizado.Total);
        Assert.Contains(actualizado.Detalles, d => d.ProductoNombre == "Té");
    }

    [Fact]
    public async Task AgregarDetalle_PedidoInexistente_DebeLanzarExcepcion()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.AgregarDetalleAsync(Guid.NewGuid(), Guid.NewGuid(), 1, 10m));
    }

    [Fact]
    public async Task AgregarDetalle_PedidoPagado_DebeLanzarExcepcion()
    {
        var mesa = new Mesa(32, 4);
        await _uot.Mesas.AgregarAsync(mesa);

        var categoria = new CategoriaProducto("Bebidas");
        await _uot.Categorias.AgregarAsync(categoria);

        var cafe = new Producto("Café", 3.00m, categoria);
        await _uot.Productos.AgregarAsync(cafe);

        await _uot.GuardarCambiosAsync();

        var pedido = await _servicio.CrearPedidoAsync(mesa.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = cafe.Id, Cantidad = 1, PrecioUnitario = 3.00m }
        });

        await _servicio.PagarPedidoAsync(pedido.Id);

        await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.AgregarDetalleAsync(pedido.Id, cafe.Id, 1, 3.00m));
    }

    // --- MarcarEnPreparacion ---

    [Fact]
    public async Task MarcarEnPreparacion_PedidoPendiente_DebeCambiarEstado()
    {
        var mesa = new Mesa(25, 4);
        await _uot.Mesas.AgregarAsync(mesa);

        var categoria = new CategoriaProducto("Bebidas");
        await _uot.Categorias.AgregarAsync(categoria);

        var producto = new Producto("Café", 3.00m, categoria);
        await _uot.Productos.AgregarAsync(producto);

        await _uot.GuardarCambiosAsync();

        var pedido = await _servicio.CrearPedidoAsync(mesa.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 3.00m }
        });

        await _servicio.MarcarEnPreparacionAsync(pedido.Id);

        var recuperado = await _servicio.ObtenerPedidoAsync(pedido.Id);
        Assert.NotNull(recuperado);
        Assert.Equal("EnPreparacion", recuperado!.Estado);
    }

    [Fact]
    public async Task MarcarEnPreparacion_Inexistente_DebeLanzarExcepcion()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.MarcarEnPreparacionAsync(Guid.NewGuid()));
    }

    // --- PagarPedido ---

    [Fact]
    [Trait("Category", "Regression")]
    public async Task PagarPedido_DesdePendiente_DebePagarCorrectamente()
    {
        var mesa = new Mesa(30, 4);
        await _uot.Mesas.AgregarAsync(mesa);

        var categoria = new CategoriaProducto("Bebidas");
        await _uot.Categorias.AgregarAsync(categoria);

        var producto = new Producto("Café", 3.00m, categoria);
        await _uot.Productos.AgregarAsync(producto);

        await _uot.GuardarCambiosAsync();

        var pedido = await _servicio.CrearPedidoAsync(mesa.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 3.00m }
        });

        await _servicio.PagarPedidoAsync(pedido.Id);

        var pagado = await _servicio.ObtenerPedidoAsync(pedido.Id);
        Assert.NotNull(pagado);
        Assert.Equal("Pagado", pagado!.Estado);
    }

    [Fact]
    public async Task PagarPedido_DesdeEnPreparacion_DebePagarCorrectamente()
    {
        var mesa = new Mesa(31, 4);
        await _uot.Mesas.AgregarAsync(mesa);

        var categoria = new CategoriaProducto("Bebidas");
        await _uot.Categorias.AgregarAsync(categoria);

        var producto = new Producto("Café", 3.00m, categoria);
        await _uot.Productos.AgregarAsync(producto);

        await _uot.GuardarCambiosAsync();

        var pedido = await _servicio.CrearPedidoAsync(mesa.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 3.00m }
        });

        await _servicio.MarcarEnPreparacionAsync(pedido.Id);
        await _servicio.PagarPedidoAsync(pedido.Id);

        var pagado = await _servicio.ObtenerPedidoAsync(pedido.Id);
        Assert.NotNull(pagado);
        Assert.Equal("Pagado", pagado!.Estado);
    }

    [Fact]
    public async Task PagarPedido_Inexistente_DebeLanzarExcepcion()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.PagarPedidoAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task PagarPedido_YaPagado_DebeLanzarExcepcion()
    {
        var mesa = new Mesa(33, 4);
        await _uot.Mesas.AgregarAsync(mesa);

        var categoria = new CategoriaProducto("Bebidas");
        await _uot.Categorias.AgregarAsync(categoria);

        var producto = new Producto("Café", 3.00m, categoria);
        await _uot.Productos.AgregarAsync(producto);

        await _uot.GuardarCambiosAsync();

        var pedido = await _servicio.CrearPedidoAsync(mesa.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 3.00m }
        });

        await _servicio.PagarPedidoAsync(pedido.Id);

        await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.PagarPedidoAsync(pedido.Id));
    }

    // --- CancelarPedido ---

    [Fact]
    [Trait("Category", "Regression")]
    public async Task CancelarPedido_Pendiente_DebeCancelarYPersistir()
    {
        var mesa = new Mesa(40, 4);
        await _uot.Mesas.AgregarAsync(mesa);

        var categoria = new CategoriaProducto("Bebidas");
        await _uot.Categorias.AgregarAsync(categoria);

        var producto = new Producto("Café", 3.00m, categoria);
        await _uot.Productos.AgregarAsync(producto);

        await _uot.GuardarCambiosAsync();

        var pedido = await _servicio.CrearPedidoAsync(mesa.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 3.00m }
        });

        await _servicio.CancelarPedidoAsync(pedido.Id);

        var cancelado = await _servicio.ObtenerPedidoAsync(pedido.Id);
        Assert.NotNull(cancelado);
        Assert.Equal("Cancelado", cancelado!.Estado);
    }

    [Fact]
    public async Task CancelarPedido_Inexistente_DebeLanzarExcepcion()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.CancelarPedidoAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task CancelarPedido_Pagado_DebeLanzarExcepcion()
    {
        var mesa = new Mesa(41, 4);
        await _uot.Mesas.AgregarAsync(mesa);

        var categoria = new CategoriaProducto("Bebidas");
        await _uot.Categorias.AgregarAsync(categoria);

        var producto = new Producto("Café", 3.00m, categoria);
        await _uot.Productos.AgregarAsync(producto);

        await _uot.GuardarCambiosAsync();

        var pedido = await _servicio.CrearPedidoAsync(mesa.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 3.00m }
        });

        await _servicio.PagarPedidoAsync(pedido.Id);

        await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.CancelarPedidoAsync(pedido.Id));
    }

    [Fact]
    public async Task CancelarPedido_Cancelado_NoApareceEnActivos()
    {
        var mesa1 = new Mesa(42, 4);
        var mesa2 = new Mesa(43, 4);
        await _uot.Mesas.AgregarAsync(mesa1);
        await _uot.Mesas.AgregarAsync(mesa2);

        var categoria = new CategoriaProducto("Bebidas");
        await _uot.Categorias.AgregarAsync(categoria);

        var producto = new Producto("Café", 3.00m, categoria);
        await _uot.Productos.AgregarAsync(producto);

        await _uot.GuardarCambiosAsync();

        var detalles = new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 3.00m }
        };

        var pedido1 = await _servicio.CrearPedidoAsync(mesa1.Id, detalles);
        var pedido2 = await _servicio.CrearPedidoAsync(mesa2.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 3.00m }
        });

        await _servicio.CancelarPedidoAsync(pedido1.Id);

        var activos = await _servicio.ListarPedidosActivosAsync();

        Assert.Single(activos);
        Assert.Equal(pedido2.Id, activos[0].Id);
        Assert.Equal("Pendiente", activos[0].Estado);
    }

    // --- EliminarDetalle ---

    [Fact]
    public async Task EliminarDetalle_ConMultiplesDetalles_DebeEliminarYRecalcularTotal()
    {
        var mesa = new Mesa(50, 4);
        await _uot.Mesas.AgregarAsync(mesa);

        var categoria = new CategoriaProducto("Bebidas");
        await _uot.Categorias.AgregarAsync(categoria);

        var cafe = new Producto("Café", 3.00m, categoria);
        var te = new Producto("Té", 2.50m, categoria);
        await _uot.Productos.AgregarAsync(cafe);
        await _uot.Productos.AgregarAsync(te);

        await _uot.GuardarCambiosAsync();

        var pedido = await _servicio.CrearPedidoAsync(mesa.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = cafe.Id, Cantidad = 2, PrecioUnitario = 3.00m },
            new() { ProductoId = te.Id, Cantidad = 1, PrecioUnitario = 2.50m }
        });
        Assert.Equal(8.50m, pedido.Total);
        Assert.Equal(2, pedido.Detalles.Count);

        var detalleAEliminar = pedido.Detalles.First(d => d.ProductoNombre == "Café");
        var actualizado = await _servicio.EliminarDetalleAsync(pedido.Id, detalleAEliminar.Id);

        Assert.Single(actualizado.Detalles);
        Assert.Equal(2.50m, actualizado.Total);
        Assert.Equal("Té", actualizado.Detalles[0].ProductoNombre);
    }

    [Fact]
    public async Task EliminarDetalle_PedidoInexistente_DebeLanzarExcepcion()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.EliminarDetalleAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task EliminarDetalle_DetalleInexistente_DebeLanzarExcepcion()
    {
        var mesa = new Mesa(51, 4);
        await _uot.Mesas.AgregarAsync(mesa);

        var categoria = new CategoriaProducto("Bebidas");
        await _uot.Categorias.AgregarAsync(categoria);

        var cafe = new Producto("Café", 3.00m, categoria);
        var te = new Producto("Té", 2.50m, categoria);
        await _uot.Productos.AgregarAsync(cafe);
        await _uot.Productos.AgregarAsync(te);

        await _uot.GuardarCambiosAsync();

        var pedido = await _servicio.CrearPedidoAsync(mesa.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = cafe.Id, Cantidad = 1, PrecioUnitario = 3.00m }
        });

        await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.EliminarDetalleAsync(pedido.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task EliminarDetalle_UltimoDetalle_DebeLanzarExcepcion()
    {
        var mesa = new Mesa(52, 4);
        await _uot.Mesas.AgregarAsync(mesa);

        var categoria = new CategoriaProducto("Bebidas");
        await _uot.Categorias.AgregarAsync(categoria);

        var cafe = new Producto("Café", 3.00m, categoria);
        await _uot.Productos.AgregarAsync(cafe);

        await _uot.GuardarCambiosAsync();

        var pedido = await _servicio.CrearPedidoAsync(mesa.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = cafe.Id, Cantidad = 1, PrecioUnitario = 3.00m }
        });

        var unicoDetalle = pedido.Detalles[0];

        await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.EliminarDetalleAsync(pedido.Id, unicoDetalle.Id));
    }

    [Fact]
    public async Task EliminarDetalle_PedidoPagado_DebeLanzarExcepcion()
    {
        var mesa = new Mesa(53, 4);
        await _uot.Mesas.AgregarAsync(mesa);

        var categoria = new CategoriaProducto("Bebidas");
        await _uot.Categorias.AgregarAsync(categoria);

        var cafe = new Producto("Café", 3.00m, categoria);
        var te = new Producto("Té", 2.50m, categoria);
        await _uot.Productos.AgregarAsync(cafe);
        await _uot.Productos.AgregarAsync(te);

        await _uot.GuardarCambiosAsync();

        var pedido = await _servicio.CrearPedidoAsync(mesa.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = cafe.Id, Cantidad = 1, PrecioUnitario = 3.00m },
            new() { ProductoId = te.Id, Cantidad = 1, PrecioUnitario = 2.50m }
        });

        await _servicio.PagarPedidoAsync(pedido.Id);

        var detalle = pedido.Detalles[0];
        await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.EliminarDetalleAsync(pedido.Id, detalle.Id));
    }

    // --- ActualizarCantidadDetalle ---

    [Fact]
    [Trait("Category", "Regression")]
    public async Task ActualizarCantidadDetalle_PedidoPendiente_DebeActualizarYRecalcularTotal()
    {
        var mesa = new Mesa(60, 4);
        await _uot.Mesas.AgregarAsync(mesa);

        var categoria = new CategoriaProducto("Bebidas");
        await _uot.Categorias.AgregarAsync(categoria);

        var cafe = new Producto("Café", 3.00m, categoria);
        await _uot.Productos.AgregarAsync(cafe);

        await _uot.GuardarCambiosAsync();

        var pedido = await _servicio.CrearPedidoAsync(mesa.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = cafe.Id, Cantidad = 2, PrecioUnitario = 3.00m }
        });
        Assert.Equal(6.00m, pedido.Total);

        var detalle = pedido.Detalles[0];

        var actualizado = await _servicio.ActualizarCantidadDetalleAsync(pedido.Id, detalle.Id, 5);

        Assert.Equal(5, actualizado.Detalles[0].Cantidad);
        Assert.Equal(15.00m, actualizado.Total);
    }

    [Fact]
    public async Task ActualizarCantidadDetalle_PedidoEnPreparacion_DebePermitir()
    {
        var mesa = new Mesa(63, 4);
        await _uot.Mesas.AgregarAsync(mesa);

        var categoria = new CategoriaProducto("Bebidas");
        await _uot.Categorias.AgregarAsync(categoria);

        var cafe = new Producto("Café", 3.00m, categoria);
        await _uot.Productos.AgregarAsync(cafe);

        await _uot.GuardarCambiosAsync();

        var pedido = await _servicio.CrearPedidoAsync(mesa.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = cafe.Id, Cantidad = 2, PrecioUnitario = 3.00m }
        });

        await _servicio.MarcarEnPreparacionAsync(pedido.Id);

        var detalle = pedido.Detalles[0];
        var actualizado = await _servicio.ActualizarCantidadDetalleAsync(pedido.Id, detalle.Id, 4);

        Assert.Equal(4, actualizado.Detalles[0].Cantidad);
        Assert.Equal(12.00m, actualizado.Total);
    }

    [Fact]
    public async Task ActualizarCantidadDetalle_PedidoInexistente_DebeLanzarExcepcion()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.ActualizarCantidadDetalleAsync(Guid.NewGuid(), Guid.NewGuid(), 3));
    }

    [Fact]
    [Trait("Category", "Regression")]
    public async Task ActualizarCantidadDetalle_PedidoPagado_DebeLanzarExcepcion()
    {
        var mesa = new Mesa(61, 4);
        await _uot.Mesas.AgregarAsync(mesa);

        var categoria = new CategoriaProducto("Bebidas");
        await _uot.Categorias.AgregarAsync(categoria);

        var cafe = new Producto("Café", 3.00m, categoria);
        await _uot.Productos.AgregarAsync(cafe);

        await _uot.GuardarCambiosAsync();

        var pedido = await _servicio.CrearPedidoAsync(mesa.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = cafe.Id, Cantidad = 2, PrecioUnitario = 3.00m }
        });

        await _servicio.PagarPedidoAsync(pedido.Id);

        var detalle = pedido.Detalles[0];
        await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.ActualizarCantidadDetalleAsync(pedido.Id, detalle.Id, 3));
    }

    [Fact]
    [Trait("Category", "Regression")]
    public async Task ActualizarCantidadDetalle_PedidoCancelado_DebeLanzarExcepcion()
    {
        var mesa = new Mesa(62, 4);
        await _uot.Mesas.AgregarAsync(mesa);

        var categoria = new CategoriaProducto("Bebidas");
        await _uot.Categorias.AgregarAsync(categoria);

        var cafe = new Producto("Café", 3.00m, categoria);
        await _uot.Productos.AgregarAsync(cafe);

        await _uot.GuardarCambiosAsync();

        var pedido = await _servicio.CrearPedidoAsync(mesa.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = cafe.Id, Cantidad = 2, PrecioUnitario = 3.00m }
        });

        await _servicio.CancelarPedidoAsync(pedido.Id);

        var detalle = pedido.Detalles[0];
        await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.ActualizarCantidadDetalleAsync(pedido.Id, detalle.Id, 3));
    }
}
