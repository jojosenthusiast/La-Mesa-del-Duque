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

public class MesasServicioTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly LaMesaDelDuqueDbContext _contexto;
    private readonly IMesasServicio _servicio;
    private readonly IUnidadDeTrabajo _uot;

    public MesasServicioTests()
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

        _servicio = new MesasServicio(_uot);
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
    }

    [Fact]
    public async Task ListarMesas_SinMesas_DebeRetornarListaVacia()
    {
        var mesas = await _servicio.ListarMesasAsync();

        Assert.NotNull(mesas);
        Assert.Empty(mesas);
    }

    [Fact]
    public async Task CrearMesa_ConDatosValidos_DebeCrearYRetornarDto()
    {
        var dto = await _servicio.CrearMesaAsync(5, 4);

        Assert.NotNull(dto);
        Assert.NotEqual(Guid.Empty, dto.Id);
        Assert.Equal(5, dto.Numero);
        Assert.Equal(4, dto.Capacidad);
        Assert.Equal("Disponible", dto.Estado);
    }

    [Fact]
    public async Task CrearMesa_ConNumeroDuplicado_DebeLanzarExcepcion()
    {
        await _servicio.CrearMesaAsync(3, 4);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _servicio.CrearMesaAsync(3, 6));
    }

    [Fact]
    public async Task ListarMesas_ConMesas_DebeRetornarTodas()
    {
        await _servicio.CrearMesaAsync(1, 2);
        await _servicio.CrearMesaAsync(2, 4);
        await _servicio.CrearMesaAsync(3, 6);

        var mesas = await _servicio.ListarMesasAsync();

        Assert.Equal(3, mesas.Count);
    }

    [Fact]
    public async Task ObtenerMesaPorNumero_Existente_DebeRetornarDto()
    {
        await _servicio.CrearMesaAsync(7, 4);

        var dto = await _servicio.ObtenerMesaPorNumeroAsync(7);

        Assert.NotNull(dto);
        Assert.Equal(7, dto!.Numero);
    }

    [Fact]
    public async Task ObtenerMesaPorNumero_Inexistente_DebeRetornarNulo()
    {
        var dto = await _servicio.ObtenerMesaPorNumeroAsync(99);

        Assert.Null(dto);
    }

    [Fact]
    public async Task CambiarEstadoMesa_CuandoExiste_DebeActualizar()
    {
        var mesa = await _servicio.CrearMesaAsync(10, 8);

        await _servicio.CambiarEstadoMesaAsync(mesa.Id, "Ocupada");

        var actualizada = await _servicio.ObtenerMesaPorNumeroAsync(10);
        Assert.NotNull(actualizada);
        Assert.Equal("Ocupada", actualizada!.Estado);
    }

    [Fact]
    public async Task CambiarEstadoMesa_AReservadaOMantenimiento_DebePersistir()
    {
        var mesa = await _servicio.CrearMesaAsync(11, 4);

        await _servicio.CambiarEstadoMesaAsync(mesa.Id, "Reservada");
        var reservada = await _servicio.ObtenerMesaPorNumeroAsync(11);

        Assert.NotNull(reservada);
        Assert.Equal("Reservada", reservada!.Estado);

        await _servicio.CambiarEstadoMesaAsync(mesa.Id, "EnMantenimiento");
        var mantenimiento = await _servicio.ObtenerMesaPorNumeroAsync(11);

        Assert.NotNull(mantenimiento);
        Assert.Equal("EnMantenimiento", mantenimiento!.Estado);
    }

    [Fact]
    public async Task CambiarEstadoMesa_CuandoNoExiste_DebeLanzarExcepcion()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.CambiarEstadoMesaAsync(Guid.NewGuid(), "Ocupada"));
    }

    [Fact]
    public async Task ActualizarMesa_ConDatosValidos_DebeActualizar()
    {
        var mesa = await _servicio.CrearMesaAsync(20, 4);

        var actualizada = await _servicio.ActualizarMesaAsync(mesa.Id, 25, 6);

        Assert.Equal(25, actualizada.Numero);
        Assert.Equal(6, actualizada.Capacidad);
    }

    [Fact]
    public async Task ActualizarMesa_Inexistente_DebeLanzarExcepcion()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.ActualizarMesaAsync(Guid.NewGuid(), 1, 4));
    }

    [Fact]
    public async Task DesactivarMesa_SinPedidosActivos_DebeDesactivar()
    {
        var mesa = await _servicio.CrearMesaAsync(30, 4);

        await _servicio.DesactivarMesaAsync(mesa.Id);

        var mesas = await _servicio.ListarMesasAsync();
        var desactivada = mesas.First(m => m.Id == mesa.Id);
        Assert.False(desactivada.Activa);
    }

    [Fact]
    public async Task DesactivarMesa_ConPedidosActivos_DebeLanzarExcepcion()
    {
        var mesa = await _servicio.CrearMesaAsync(40, 4);

        // Re-obtener mesa con tracking
        var mesaTracked = await _uot.Mesas.ObtenerParaActualizarAsync(mesa.Id);
        var categoria = new CategoriaProducto("Bebidas");
        await _uot.Categorias.AgregarAsync(categoria);
        var producto = new Producto("Café", 3.00m, categoria);
        await _uot.Productos.AgregarAsync(producto);
        await _uot.GuardarCambiosAsync();

        var pedido = new Pedido(TipoServicio.ComerAqui, mesaTracked!);
        pedido.AgregarDetalle(new DetallePedido(producto, 1, 3.00m));
        await _uot.Pedidos.AgregarAsync(pedido);
        await _uot.GuardarCambiosAsync();

        await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.DesactivarMesaAsync(mesa.Id));
    }

    [Fact]
    public async Task CambiarEstadoMesa_ADisponible_ConPedidosActivos_DebeLanzarExcepcion()
    {
        var mesa = await _servicio.CrearMesaAsync(41, 4);

        var mesaTracked = await _uot.Mesas.ObtenerParaActualizarAsync(mesa.Id);
        mesaTracked!.Ocupar();
        var categoria = new CategoriaProducto("Bebidas activas");
        await _uot.Categorias.AgregarAsync(categoria);
        var producto = new Producto("Café activo", 3.00m, categoria);
        await _uot.Productos.AgregarAsync(producto);
        await _uot.GuardarCambiosAsync();

        var pedido = new Pedido(TipoServicio.ComerAqui, mesaTracked!);
        pedido.AgregarDetalle(new DetallePedido(producto, 1, 3.00m));
        await _uot.Pedidos.AgregarAsync(pedido);
        await _uot.GuardarCambiosAsync();

        await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.CambiarEstadoMesaAsync(mesa.Id, "Disponible"));
    }

    [Fact]
    public async Task DesactivarMesa_Inexistente_DebeLanzarExcepcion()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.DesactivarMesaAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task CrearMesa_DebeEstarActivaPorDefecto()
    {
        var dto = await _servicio.CrearMesaAsync(50, 2);

        Assert.True(dto.Activa);
    }

    [Fact]
    public async Task CambiarEstadoMesa_A_Ocupada_DebeSetearOcupadaDesde()
    {
        var mesa = await _servicio.CrearMesaAsync(60, 4);

        await _servicio.CambiarEstadoMesaAsync(mesa.Id, "Ocupada");

        var actualizada = await _servicio.ObtenerMesaPorNumeroAsync(60);
        Assert.NotNull(actualizada);
        Assert.Equal("Ocupada", actualizada!.Estado);
        Assert.NotNull(actualizada.OcupadaDesde);
    }

    [Fact]
    public async Task CambiarEstadoMesa_A_Disponible_Desde_Ocupada_DebeLimpiarOcupadaDesde()
    {
        var mesa = await _servicio.CrearMesaAsync(61, 4);
        await _servicio.CambiarEstadoMesaAsync(mesa.Id, "Ocupada");

        await _servicio.CambiarEstadoMesaAsync(mesa.Id, "Disponible");

        var actualizada = await _servicio.ObtenerMesaPorNumeroAsync(61);
        Assert.NotNull(actualizada);
        Assert.Equal("Disponible", actualizada!.Estado);
        Assert.Null(actualizada.OcupadaDesde);
    }

    [Fact]
    public async Task ActualizarPosicion_CuandoDatosSonValidos_DebeActualizarCampos()
    {
        var mesa = await _servicio.CrearMesaAsync(70, 4);
        var zona = new ZonaSalon("Terraza");
        await _uot.ZonasSalon.AgregarAsync(zona);
        await _uot.GuardarCambiosAsync();

        var actualizada = await _servicio.ActualizarPosicionAsync(mesa.Id, 50, 75, zona.Id, "Redonda", 45);

        Assert.Equal(50, actualizada.PosicionX);
        Assert.Equal(75, actualizada.PosicionY);
        Assert.Equal(zona.Id, actualizada.ZonaId);
        Assert.Equal("Redonda", actualizada.Forma);
        Assert.Equal(45, actualizada.Rotacion);
    }

    [Fact]
    public async Task ActualizarPosicion_CuandoFormaEsInvalida_DebeLanzarExcepcion()
    {
        var mesa = await _servicio.CrearMesaAsync(71, 4);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.ActualizarPosicionAsync(mesa.Id, 10, 20, Guid.NewGuid(), "Triangular"));
    }

    [Fact]
    public async Task ActualizarPosicion_CuandoMesaNoExiste_DebeLanzarExcepcion()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.ActualizarPosicionAsync(Guid.NewGuid(), 10, 20, Guid.NewGuid(), "Redonda"));
    }

    [Fact]
    public async Task LimpiarPosicion_CuandoExiste_DebeDejarCamposNulos()
    {
        var mesa = await _servicio.CrearMesaAsync(72, 4);
        var zona = new ZonaSalon("Terraza");
        await _uot.ZonasSalon.AgregarAsync(zona);
        await _uot.GuardarCambiosAsync();
        await _servicio.ActualizarPosicionAsync(mesa.Id, 50, 75, zona.Id, "Bar", 90);

        var limpiada = await _servicio.LimpiarPosicionAsync(mesa.Id);

        Assert.Null(limpiada.PosicionX);
        Assert.Null(limpiada.PosicionY);
        Assert.Null(limpiada.ZonaId);
        Assert.Null(limpiada.Forma);
        Assert.Null(limpiada.Rotacion);
    }

    [Fact]
    public async Task ListarMesas_DebeIncluirCamposDePosicion()
    {
        var mesa = await _servicio.CrearMesaAsync(80, 4);
        var zona = new ZonaSalon("Terraza");
        await _uot.ZonasSalon.AgregarAsync(zona);
        await _uot.GuardarCambiosAsync();
        await _servicio.ActualizarPosicionAsync(mesa.Id, 10, 20, zona.Id, "Cuadrada", 0);

        var mesas = await _servicio.ListarMesasAsync();
        var dto = mesas.First(m => m.Id == mesa.Id);

        Assert.Equal(10, dto.PosicionX);
        Assert.Equal(20, dto.PosicionY);
        Assert.Equal(zona.Id, dto.ZonaId);
        Assert.Equal("Cuadrada", dto.Forma);
        Assert.Equal(0, dto.Rotacion);
    }
}
