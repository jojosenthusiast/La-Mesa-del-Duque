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

public class PedidosServicioTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly LaMesaDelDuqueDbContext _contexto;
    private readonly IPedidosServicio _servicio;
    private readonly IUnidadDeTrabajo _uot;
    private readonly NotificadorPedidosSpy _notificadorSpy;

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

        _notificadorSpy = new NotificadorPedidosSpy();
        _servicio = new PedidosServicio(_uot, _notificadorSpy);
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
    }

    private async Task<(Mesa mesa, Producto producto)> CrearMesaYProductoAsync(int numeroMesa = 1)
    {
        var mesa = new Mesa(numeroMesa, 4);
        var categoria = new CategoriaProducto($"Bebidas {numeroMesa}");
        var producto = new Producto($"Café {numeroMesa}", 3.50m, categoria);

        await _uot.Mesas.AgregarAsync(mesa);
        await _uot.Categorias.AgregarAsync(categoria);
        await _uot.Productos.AgregarAsync(producto);
        await _uot.GuardarCambiosAsync();

        return (mesa, producto);
    }

    private async Task<Usuario> CrearUsuarioAuditoriaAsync()
    {
        var rol = new Rol("admin", "Administrador");
        var usuario = new Usuario("admin", "admin@lmd.test", "hash-demo", "Admin Demo", rol);

        _contexto.Set<Rol>().Add(rol);
        _contexto.Set<Usuario>().Add(usuario);
        await _contexto.SaveChangesAsync();

        return usuario;
    }

    [Fact]
    public async Task CrearPedido_ParaLlevar_SinMesa_DebeCrearPedidoPendiente()
    {
        var (_, producto) = await CrearMesaYProductoAsync();

        var pedido = await _servicio.CrearPedidoAsync(TipoServicio.ParaLlevar, null, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 2, PrecioUnitario = 3.50m }
        });

        Assert.Equal("Pendiente", pedido.Estado);
        Assert.Equal("ParaLlevar", pedido.TipoServicio);
        Assert.Null(pedido.MesaId);
        Assert.Null(pedido.MesaNumero);
        Assert.Equal(7.00m, pedido.Total);
    }

    [Fact]
    public async Task CrearPedido_ComerAqui_ConMesa_DebeAsignarMesaYOcuparla()
    {
        var (mesa, producto) = await CrearMesaYProductoAsync(10);

        var pedido = await _servicio.CrearPedidoAsync(TipoServicio.ComerAqui, mesa.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 3.50m }
        });

        var mesaActualizada = await _uot.Mesas.ObtenerPorIdAsync(mesa.Id);

        Assert.Equal("ComerAqui", pedido.TipoServicio);
        Assert.Equal(mesa.Id, pedido.MesaId);
        Assert.Equal(10, pedido.MesaNumero);
        Assert.NotNull(mesaActualizada);
        Assert.Equal(EstadoMesa.Ocupada, mesaActualizada!.Estado);
    }

    [Fact]
    public async Task CrearPedido_ParaLlevar_ConMesa_DebeLanzarExcepcion()
    {
        var (mesa, producto) = await CrearMesaYProductoAsync(11);

        await Assert.ThrowsAsync<ReglaDominioException>(() => _servicio.CrearPedidoAsync(TipoServicio.ParaLlevar, mesa.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 3.50m }
        }));
    }

    [Fact]
    public async Task ActualizarCantidadDetalle_PedidoEnPreparacion_DebePermitir()
    {
        var (mesa, producto) = await CrearMesaYProductoAsync(12);

        var pedido = await _servicio.CrearPedidoAsync(TipoServicio.ComerAqui, mesa.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 2, PrecioUnitario = 3.00m }
        });

        await _servicio.MarcarEnPreparacionAsync(pedido.Id);

        var actualizado = await _servicio.ActualizarCantidadDetalleAsync(pedido.Id, pedido.Detalles[0].Id, 4);

        Assert.Equal(4, actualizado.Detalles[0].Cantidad);
        Assert.Equal(12.00m, actualizado.Total);
    }

    [Fact]
    public async Task EliminarPedidoPendiente_DebeEliminarRegistro()
    {
        var (_, producto) = await CrearMesaYProductoAsync(13);
        var usuario = await CrearUsuarioAuditoriaAsync();

        var pedido = await _servicio.CrearPedidoAsync(TipoServicio.ParaLlevar, null, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 3.50m }
        });

        await _servicio.EliminarPedidoPendienteAsync(pedido.Id, usuario.Id);

        var eliminado = await _servicio.ObtenerPedidoAsync(pedido.Id);
        Assert.Null(eliminado);
    }

    [Fact]
    public async Task EliminarPedidoPendiente_ConMesa_DebeLiberarMesa()
    {
        var (mesa, producto) = await CrearMesaYProductoAsync(14);
        var usuario = await CrearUsuarioAuditoriaAsync();

        var pedido = await _servicio.CrearPedidoAsync(TipoServicio.ComerAqui, mesa.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 3.50m }
        });

        await _servicio.EliminarPedidoPendienteAsync(pedido.Id, usuario.Id);

        var mesaLiberada = await _uot.Mesas.ObtenerPorIdAsync(mesa.Id);
        Assert.NotNull(mesaLiberada);
        Assert.Equal(EstadoMesa.Disponible, mesaLiberada!.Estado);
    }

    [Fact]
    public async Task EliminarPedidoPendiente_DebeRegistrarAuditoria()
    {
        var (_, producto) = await CrearMesaYProductoAsync(15);
        var usuario = await CrearUsuarioAuditoriaAsync();

        var pedido = await _servicio.CrearPedidoAsync(TipoServicio.ParaLlevar, null, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 3.50m }
        });

        await _servicio.EliminarPedidoPendienteAsync(pedido.Id, usuario.Id, "127.0.0.1");

        var auditoria = await _contexto.Set<Auditoria>().SingleAsync();
        Assert.Equal("pedido", auditoria.TablaAfectada);
        Assert.Equal(pedido.Id, auditoria.RegistroId);
        Assert.Equal("DELETE", auditoria.Accion);
        Assert.Equal(usuario.Id, auditoria.UsuarioId);
        Assert.Equal("127.0.0.1", auditoria.IpAddress);
    }

    [Fact]
    public async Task EliminarPedidoPendiente_Pagado_DebeRechazarOperacion()
    {
        var (_, producto) = await CrearMesaYProductoAsync(16);
        var usuario = await CrearUsuarioAuditoriaAsync();

        var pedido = await _servicio.CrearPedidoAsync(TipoServicio.ParaLlevar, null, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 3.50m }
        });

        await _servicio.PagarPedidoAsync(pedido.Id);

        await Assert.ThrowsAsync<ReglaDominioException>(() => _servicio.EliminarPedidoPendienteAsync(pedido.Id, usuario.Id));
    }

    [Fact]
    public async Task ListarPedidosActivos_DebeIncluirPendientesYEnPreparacion()
    {
        var (_, producto1) = await CrearMesaYProductoAsync(17);
        var (_, producto2) = await CrearMesaYProductoAsync(18);

        var pendiente = await _servicio.CrearPedidoAsync(TipoServicio.ParaLlevar, null, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto1.Id, Cantidad = 1, PrecioUnitario = 3.50m }
        });

        var enPreparacion = await _servicio.CrearPedidoAsync(TipoServicio.ParaLlevar, null, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto2.Id, Cantidad = 1, PrecioUnitario = 3.50m }
        });

        await _servicio.MarcarEnPreparacionAsync(enPreparacion.Id);

        var activos = await _servicio.ListarPedidosActivosAsync();

        Assert.Contains(activos, x => x.Id == pendiente.Id);
        Assert.Contains(activos, x => x.Id == enPreparacion.Id);
    }

    [Fact]
    public async Task PagarPedido_UnicoPedidoEnMesa_DebeLiberarMesa()
    {
        var (mesa, producto) = await CrearMesaYProductoAsync(30);

        var pedido = await _servicio.CrearPedidoAsync(TipoServicio.ComerAqui, mesa.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 3.50m }
        });

        await _servicio.PagarPedidoAsync(pedido.Id);

        var mesaActualizada = await _uot.Mesas.ObtenerPorIdAsync(mesa.Id);
        Assert.NotNull(mesaActualizada);
        Assert.Equal(EstadoMesa.Disponible, mesaActualizada!.Estado);
    }

    [Fact]
    public async Task CancelarPedido_UnicoPedidoEnMesa_DebeLiberarMesa()
    {
        var (mesa, producto) = await CrearMesaYProductoAsync(31);

        var pedido = await _servicio.CrearPedidoAsync(TipoServicio.ComerAqui, mesa.Id, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 3.50m }
        });

        await _servicio.CancelarPedidoAsync(pedido.Id);

        var mesaActualizada = await _uot.Mesas.ObtenerPorIdAsync(mesa.Id);
        Assert.NotNull(mesaActualizada);
        Assert.Equal(EstadoMesa.Disponible, mesaActualizada!.Estado);
    }

    [Fact]
    public async Task CrearPedido_DebeEmitirNotificacionDeCreacion()
    {
        var (_, producto) = await CrearMesaYProductoAsync(40);

        var pedido = await _servicio.CrearPedidoAsync(TipoServicio.ParaLlevar, null, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 3.50m }
        });

        var notificacion = Assert.Single(_notificadorSpy.PedidosCreados);
        Assert.Equal(pedido.Id, notificacion.PedidoId);
        Assert.Equal(EstadoPedido.Pendiente, notificacion.Estado);
    }

    [Fact]
    public async Task MarcarEnPreparacion_DebeEmitirNotificacionDeCambioEstado()
    {
        var (_, producto) = await CrearMesaYProductoAsync(41);

        var pedido = await _servicio.CrearPedidoAsync(TipoServicio.ParaLlevar, null, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 3.50m }
        });

        _notificadorSpy.EstadosCambiados.Clear();

        await _servicio.MarcarEnPreparacionAsync(pedido.Id);

        var notificacion = Assert.Single(_notificadorSpy.EstadosCambiados);
        Assert.Equal(pedido.Id, notificacion.PedidoId);
        Assert.Equal(EstadoPedido.EnPreparacion, notificacion.Estado);
    }

    [Fact]
    public async Task CancelarPedido_DebeEmitirNotificacionDeCancelacion()
    {
        var (_, producto) = await CrearMesaYProductoAsync(42);

        var pedido = await _servicio.CrearPedidoAsync(TipoServicio.ParaLlevar, null, new List<DetalleCreacionDto>
        {
            new() { ProductoId = producto.Id, Cantidad = 1, PrecioUnitario = 3.50m }
        });

        _notificadorSpy.PedidosCancelados.Clear();

        await _servicio.CancelarPedidoAsync(pedido.Id);

        var notificacion = Assert.Single(_notificadorSpy.PedidosCancelados);
        Assert.Equal(pedido.Id, notificacion);
    }
}
