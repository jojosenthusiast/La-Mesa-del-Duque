using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using LaMesaDelDuque.Infraestructura.Repositorios;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Pruebas.Aplicacion;

public class PedidosServicioStockTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly LaMesaDelDuqueDbContext _contexto;
    private readonly IUnidadDeTrabajo _uot;
    private readonly IPedidosServicio _servicio;

    public PedidosServicioStockTests()
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
            new IngredienteRepositorio(_contexto),
            new MesaRepositorio(_contexto),
            new PedidoRepositorio(_contexto),
            new RolRepositorio(_contexto),
            new UsuarioRepositorio(_contexto),
            new AuditoriaRepositorio(_contexto),
            new RecetaProductoRepositorio(_contexto),
            new OrdenCocinaRepositorio(_contexto),
            new CuentaRepositorio(_contexto),
            new PagoRepositorio(_contexto),
            new ZonaSalonRepositorio(_contexto));

        var rolCaja = new Rol("Cajero");
        var usuarioCaja = new Usuario("cajero-stock", "cajero-stock@lmd.test", "hash-demo", "Cajero Stock", rolCaja);
        _contexto.Set<Rol>().Add(rolCaja);
        _contexto.Set<Usuario>().Add(usuarioCaja);
        _contexto.Set<CierreDia>().Add(new CierreDia(DateOnly.FromDateTime(DateTime.UtcNow), 0, 0, 0, 0, 0, 0, usuarioCaja));
        _contexto.Set<TurnoCaja>().Add(new TurnoCaja(usuarioCaja.Id, 100m));
        _contexto.SaveChanges();

        _servicio = new PedidosServicio(
            _uot,
            new NotificadorPedidosSpy(),
            httpContextAccessor: TestHttpContextAccessor.ConUsuarioAutenticado(usuarioCaja.Id, "Cajero"));
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
    }

    [Fact]
    public async Task CrearPedido_CuandoIngredienteSinStock_DebeRechazarYNoCrearPedido()
    {
        var (producto, ingrediente) = await CrearProductoConRecetaAsync("Pizza agotada", stockIngrediente: 0m, consumoPorUnidad: 1m);

        var ex = await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.CrearPedidoAsync(TipoServicio.ParaLlevar, null, new List<DetalleCreacionDto>
            {
                new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = producto.Precio }
            }));

        Assert.Contains("Pizza agotada", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Queso Pizza agotada", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, await _contexto.Set<Pedido>().CountAsync());
        Assert.Equal(0m, await StockActualAsync(ingrediente.Id));
    }

    [Fact]
    public async Task CrearPedido_CuandoDosLineasCompartenIngredienteYSuperanStock_DebeRechazarSinDescontar()
    {
        var (producto, ingrediente) = await CrearProductoConRecetaAsync("Pizza doble", stockIngrediente: 1m, consumoPorUnidad: 1m);

        var ex = await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.CrearPedidoAsync(TipoServicio.ParaLlevar, null, new List<DetalleCreacionDto>
            {
                new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = producto.Precio },
                new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = producto.Precio }
            }));

        Assert.Contains("Pizza doble", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, await _contexto.Set<Pedido>().CountAsync());
        Assert.Equal(1m, await StockActualAsync(ingrediente.Id));
    }

    [Fact]
    public async Task CrearPedido_CuandoStockAlcanzaExacto_DebeDescontarHastaCeroSinNegativo()
    {
        var (producto, ingrediente) = await CrearProductoConRecetaAsync("Pizza exacta", stockIngrediente: 2m, consumoPorUnidad: 1m);

        var pedido = await _servicio.CrearPedidoAsync(TipoServicio.ParaLlevar, null, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 2, PrecioUnitario = producto.Precio }
        });

        Assert.Equal("EnPreparacion", pedido.Estado);
        Assert.Equal(0m, await StockActualAsync(ingrediente.Id));
    }

    [Fact]
    public async Task CrearPedido_CuandoStockLlegaACero_DebeDesactivarProductoParaPOS()
    {
        var (producto, _) = await CrearProductoConRecetaAsync("Pizza ultimo stock", stockIngrediente: 1m, consumoPorUnidad: 1m);

        await _servicio.CrearPedidoAsync(TipoServicio.ParaLlevar, null, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = producto.Precio }
        });

        var productoActualizado = await _contexto.Set<Producto>()
            .AsNoTracking()
            .SingleAsync(p => p.Id == producto.Id);
        Assert.False(productoActualizado.Activo);
    }

    [Fact]
    public async Task ActualizarCantidadDetalle_CuandoIncrementoSuperaStock_DebeRechazarYConservarStock()
    {
        var (producto, ingrediente) = await CrearProductoConRecetaAsync("Pizza incremento", stockIngrediente: 2m, consumoPorUnidad: 1m);
        var pedido = await _servicio.CrearPedidoAsync(TipoServicio.ParaLlevar, null, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = producto.Precio }
        });

        var ex = await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.ActualizarCantidadDetalleAsync(pedido.Id, pedido.Detalles[0].Id, 3));

        Assert.Contains("Pizza incremento", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1m, await StockActualAsync(ingrediente.Id));

        var detallePersistido = await _contexto.Set<DetallePedido>().SingleAsync();
        Assert.Equal(1, detallePersistido.Cantidad);
    }

    [Fact]
    public async Task AgregarDetalle_CuandoPedidoEstaPagado_DebeRechazarSinReservarStock()
    {
        var (producto, ingrediente) = await CrearProductoConRecetaAsync("Pizza pagada", stockIngrediente: 2m, consumoPorUnidad: 1m);
        var pedido = await _servicio.CrearPedidoAsync(TipoServicio.ParaLlevar, null, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = producto.Precio }
        });
        await _servicio.PagarPedidoAsync(pedido.Id);

        var ex = await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.AgregarDetalleAsync(pedido.Id, producto.Id, 1, producto.Precio));

        Assert.Contains("pedido pagado", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1m, await StockActualAsync(ingrediente.Id));

        var ingredienteTrackeado = _contexto.ChangeTracker.Entries<Ingrediente>()
            .Single(e => e.Entity.Id == ingrediente.Id)
            .Entity;
        Assert.Equal(1m, ingredienteTrackeado.StockActual);
    }

    [Fact]
    public async Task EliminarPedidoPendiente_CuandoPedidoReservoStock_DebeDevolverStockAntesDeEliminar()
    {
        var (producto, ingrediente) = await CrearProductoConRecetaAsync("Pizza eliminada", stockIngrediente: 2m, consumoPorUnidad: 1m);
        var pedido = await _servicio.CrearPedidoAsync(TipoServicio.ParaLlevar, null, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = producto.Precio }
        });

        var usuarioId = await _contexto.Set<Usuario>()
            .Select(u => u.Id)
            .SingleAsync();
        await _servicio.EliminarPedidoPendienteAsync(pedido.Id, usuarioId);

        Assert.Equal(0, await _contexto.Set<Pedido>().CountAsync());
        Assert.Equal(2m, await StockActualAsync(ingrediente.Id));
    }

    [Fact]
    public async Task CancelarPedido_CuandoDevuelveStock_NoDebeReactivarProductoDesactivadoPorStockAutomaticamente()
    {
        var (producto, ingrediente) = await CrearProductoConRecetaAsync("Pizza cancelada", stockIngrediente: 1m, consumoPorUnidad: 1m);
        var pedido = await _servicio.CrearPedidoAsync(TipoServicio.ParaLlevar, null, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = producto.Precio }
        });

        await _servicio.CancelarPedidoAsync(pedido.Id);

        Assert.Equal(1m, await StockActualAsync(ingrediente.Id));
        var productoActualizado = await _contexto.Set<Producto>()
            .AsNoTracking()
            .SingleAsync(p => p.Id == producto.Id);
        Assert.False(productoActualizado.Activo);
    }

    [Fact]
    public async Task ActualizarCantidadDetalle_CuandoCantidadInvalida_DebeRechazarSinDevolverStockLocalmente()
    {
        var (producto, ingrediente) = await CrearProductoConRecetaAsync("Pizza cantidad invalida", stockIngrediente: 2m, consumoPorUnidad: 1m);
        var pedido = await _servicio.CrearPedidoAsync(TipoServicio.ParaLlevar, null, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = producto.Precio }
        });

        var ex = await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.ActualizarCantidadDetalleAsync(pedido.Id, pedido.Detalles[0].Id, 0));

        Assert.Contains("cantidad", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1m, await StockActualAsync(ingrediente.Id));

        var ingredienteTrackeado = _contexto.ChangeTracker.Entries<Ingrediente>()
            .Single(e => e.Entity.Id == ingrediente.Id)
            .Entity;
        Assert.Equal(1m, ingredienteTrackeado.StockActual);
    }

    [Fact]
    public async Task GuardarCambiosAsync_CuandoOtroContextoYaConsumioStock_DebeLanzarConcurrenciaExceptionYConservarStock()
    {
        var (_, ingrediente) = await CrearProductoConRecetaAsync("Pizza concurrencia", stockIngrediente: 1m, consumoPorUnidad: 1m);
        var opciones = new DbContextOptionsBuilder<LaMesaDelDuqueDbContext>()
            .UseSqlite(_conexion)
            .Options;

        await using var contextoA = new LaMesaDelDuqueDbContext(opciones);
        await using var contextoB = new LaMesaDelDuqueDbContext(opciones);
        var uotA = CrearUnidadDeTrabajo(contextoA);
        var uotB = CrearUnidadDeTrabajo(contextoB);

        var ingredienteA = await contextoA.Set<Ingrediente>().SingleAsync(i => i.Id == ingrediente.Id);
        var ingredienteB = await contextoB.Set<Ingrediente>().SingleAsync(i => i.Id == ingrediente.Id);

        ingredienteA.DescontarStock(1m);
        ingredienteB.DescontarStock(1m);

        await uotA.GuardarCambiosAsync();
        await Assert.ThrowsAsync<ConcurrenciaException>(() => uotB.GuardarCambiosAsync());

        Assert.Equal(0m, await StockActualAsync(ingrediente.Id));
    }

    private async Task<(Producto producto, Ingrediente ingrediente)> CrearProductoConRecetaAsync(
        string nombreProducto,
        decimal stockIngrediente,
        decimal consumoPorUnidad)
    {
        var categoria = new CategoriaProducto($"Categoría {nombreProducto}");
        var producto = new Producto(nombreProducto, 10m, categoria);
        var ingrediente = new Ingrediente($"Queso {nombreProducto}", "kg", stockIngrediente, 0m, 3m);
        var receta = new RecetaProducto(producto, "Preparar y hornear.", new[]
        {
            new RecetaIngrediente(ingrediente, consumoPorUnidad)
        });

        await _contexto.Set<CategoriaProducto>().AddAsync(categoria);
        await _contexto.Set<Producto>().AddAsync(producto);
        await _contexto.Set<Ingrediente>().AddAsync(ingrediente);
        await _contexto.Set<RecetaProducto>().AddAsync(receta);
        await _contexto.SaveChangesAsync();

        return (producto, ingrediente);
    }

    private static IUnidadDeTrabajo CrearUnidadDeTrabajo(LaMesaDelDuqueDbContext contexto)
    {
        return new UnidadDeTrabajo(contexto,
            new CategoriaProductoRepositorio(contexto),
            new ProductoRepositorio(contexto),
            new IngredienteRepositorio(contexto),
            new MesaRepositorio(contexto),
            new PedidoRepositorio(contexto),
            new RolRepositorio(contexto),
            new UsuarioRepositorio(contexto),
            new AuditoriaRepositorio(contexto),
            new RecetaProductoRepositorio(contexto),
            new OrdenCocinaRepositorio(contexto),
            new CuentaRepositorio(contexto),
            new PagoRepositorio(contexto),
            new ZonaSalonRepositorio(contexto));
    }

    private async Task<decimal> StockActualAsync(Guid ingredienteId)
    {
        return await _contexto.Set<Ingrediente>()
            .AsNoTracking()
            .Where(i => i.Id == ingredienteId)
            .Select(i => i.StockActual)
            .SingleAsync();
    }
}
