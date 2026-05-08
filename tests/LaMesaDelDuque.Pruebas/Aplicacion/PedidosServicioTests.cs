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

    [Fact]
    [Trait("Category", "Regression")]
    public async Task CrearPedido_ConUnDetalle_DebeCrearYRetornarDtoConTotal()
    {
        // Arrange: crear mesa y producto
        var mesa = new Mesa(1, 4);
        await _uot.Mesas.AgregarAsync(mesa);

        var categoria = new CategoriaProducto("Bebidas");
        await _uot.Categorias.AgregarAsync(categoria);

        var producto = new Producto("Café", 3.50m, categoria);
        await _uot.Productos.AgregarAsync(producto);

        await _uot.GuardarCambiosAsync();

        // Act
        var detalles = new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 2, PrecioUnitario = 3.50m }
        };
        var pedidoDto = await _servicio.CrearPedidoAsync(mesa.Id, detalles);

        // Assert
        Assert.NotNull(pedidoDto);
        Assert.NotEqual(Guid.Empty, pedidoDto.Id);
        Assert.Equal(mesa.Id, pedidoDto.MesaId);
        Assert.Equal(mesa.Numero, pedidoDto.MesaNumero);
        Assert.Equal("Abierto", pedidoDto.Estado);
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
        // Arrange
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

        // Act
        var pedidoDto = await _servicio.CrearPedidoAsync(mesa.Id, detalles);

        // Assert: 8.00 + (6.50 * 2) = 21.00
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

    [Fact]
    public async Task ObtenerPedido_Existente_DebeIncluirDetallesProductoYMesa()
    {
        // Arrange: crear pedido con detalle
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

        // Act
        var obtenido = await _servicio.ObtenerPedidoAsync(dtoCreado.Id);

        // Assert
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

    [Fact]
    public async Task ListarPedidosActivos_DebeIncluirSoloAbiertos()
    {
        // Arrange: crear 2 pedidos abiertos y 1 cerrado
        var mesa1 = new Mesa(10, 4);
        var mesa2 = new Mesa(11, 4);
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

        var abierto1 = await _servicio.CrearPedidoAsync(mesa1.Id, detalles);
        var abierto2 = await _servicio.CrearPedidoAsync(mesa2.Id, detalles);

        // Cerrar el segundo pedido
        await _servicio.CerrarPedidoAsync(abierto2.Id);

        // Act
        var activos = await _servicio.ListarPedidosActivosAsync();

        // Assert
        Assert.Single(activos);
        Assert.Equal(abierto1.Id, activos[0].Id);
        Assert.Equal("Abierto", activos[0].Estado);
    }

    [Fact]
    public async Task ListarPedidosActivos_SinPedidos_DebeRetornarListaVacia()
    {
        var activos = await _servicio.ListarPedidosActivosAsync();

        Assert.NotNull(activos);
        Assert.Empty(activos);
    }

    [Fact]
    public async Task AgregarDetalle_PedidoAbierto_DebeAgregarYRecalcularTotal()
    {
        // Arrange: crear pedido con 1 detalle
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

        // Act: agregar otro detalle
        var actualizado = await _servicio.AgregarDetalleAsync(pedido.Id, te.Id, 1, 2.50m);

        // Assert
        Assert.Equal(2, actualizado.Detalles.Count);
        Assert.Equal(8.50m, actualizado.Total); // 6.00 + 2.50
        Assert.Contains(actualizado.Detalles, d => d.ProductoNombre == "Té");
    }

    [Fact]
    public async Task AgregarDetalle_PedidoInexistente_DebeLanzarExcepcion()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.AgregarDetalleAsync(Guid.NewGuid(), Guid.NewGuid(), 1, 10m));
    }

    [Fact]
    public async Task CerrarPedido_ConDetalles_DebeCerrarCorrectamente()
    {
        // Arrange
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

        // Act
        await _servicio.CerrarPedidoAsync(pedido.Id);

        // Assert: el pedido ahora debe estar cerrado
        var cerrado = await _servicio.ObtenerPedidoAsync(pedido.Id);
        Assert.NotNull(cerrado);
        Assert.Equal("Cerrado", cerrado!.Estado);
    }

    [Fact]
    public async Task CerrarPedido_Inexistente_DebeLanzarExcepcion()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.CerrarPedidoAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task CerrarPedido_YaCerrado_DebeLanzarExcepcion()
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

        await _servicio.CerrarPedidoAsync(pedido.Id);

        // Segundo cierre debe lanzar excepción del dominio
        await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.CerrarPedidoAsync(pedido.Id));
    }

    [Fact]
    public async Task AgregarDetalle_PedidoCerrado_DebeLanzarExcepcion()
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

        await _servicio.CerrarPedidoAsync(pedido.Id);

        // Agregar detalle a pedido cerrado debe lanzar excepción del dominio
        await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.AgregarDetalleAsync(pedido.Id, cafe.Id, 1, 3.00m));
    }

    [Fact]
    [Trait("Category", "Regression")]
    public async Task CancelarPedido_Abierto_DebeCancelarYPersistir()
    {
        // Arrange
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

        // Act
        await _servicio.CancelarPedidoAsync(pedido.Id);

        // Assert: el pedido ahora debe estar cancelado
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
    public async Task CancelarPedido_Cerrado_DebeLanzarExcepcion()
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

        await _servicio.CerrarPedidoAsync(pedido.Id);

        // Cancelar pedido cerrado debe lanzar excepción del dominio
        await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.CancelarPedidoAsync(pedido.Id));
    }

    [Fact]
    public async Task CancelarPedido_Cancelado_NoApareceEnActivos()
    {
        // Arrange: crear 2 pedidos, cancelar uno
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

        // Cancelar el primer pedido
        await _servicio.CancelarPedidoAsync(pedido1.Id);

        // Act
        var activos = await _servicio.ListarPedidosActivosAsync();

        // Assert: solo debe aparecer el pedido abierto (no cancelado)
        Assert.Single(activos);
        Assert.Equal(pedido2.Id, activos[0].Id);
        Assert.Equal("Abierto", activos[0].Estado);
    }

    [Fact]
    public async Task EliminarDetalle_ConMultiplesDetalles_DebeEliminarYRecalcularTotal()
    {
        // Arrange: crear pedido con 2 detalles
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

        // Act: eliminar el primer detalle
        var detalleAEliminar = pedido.Detalles.First(d => d.ProductoNombre == "Café");
        var actualizado = await _servicio.EliminarDetalleAsync(pedido.Id, detalleAEliminar.Id);

        // Assert
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
        // Arrange: crear pedido con un detalle
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

        // Intentar eliminar un detalle que no pertenece al pedido (ID inventado)
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
    public async Task EliminarDetalle_PedidoCerrado_DebeLanzarExcepcion()
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

        await _servicio.CerrarPedidoAsync(pedido.Id);

        var detalle = pedido.Detalles[0];
        await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.EliminarDetalleAsync(pedido.Id, detalle.Id));
    }

    [Fact]
    [Trait("Category", "Regression")]
    public async Task ActualizarCantidadDetalle_PedidoAbierto_DebeActualizarYRecalcularTotal()
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

        // Act: cambiar cantidad de 2 a 5
        var actualizado = await _servicio.ActualizarCantidadDetalleAsync(pedido.Id, detalle.Id, 5);

        Assert.Equal(5, actualizado.Detalles[0].Cantidad);
        Assert.Equal(15.00m, actualizado.Total);
    }

    [Fact]
    public async Task ActualizarCantidadDetalle_PedidoInexistente_DebeLanzarExcepcion()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.ActualizarCantidadDetalleAsync(Guid.NewGuid(), Guid.NewGuid(), 3));
    }

    [Fact]
    [Trait("Category", "Regression")]
    public async Task ActualizarCantidadDetalle_PedidoCerrado_DebeLanzarExcepcion()
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

        await _servicio.CerrarPedidoAsync(pedido.Id);

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
