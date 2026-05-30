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

public class CierreServicioTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly LaMesaDelDuqueDbContext _contexto;
    private readonly IUnidadDeTrabajo _uot;
    private readonly ICierreServicio _servicio;

    public CierreServicioTests()
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
            new PromocionRepositorio(_contexto),
            new TurnoCajaRepositorio(_contexto),
            new DescuentoRepositorio(_contexto),
            new MotivoDescuentoRepositorio(_contexto),
            new DevolucionRepositorio(_contexto),
            null,
            new MermaRepositorio(_contexto),
            new CierreDiaRepositorio(_contexto));

        var mermaServicio = new MermaServicio(_uot);
        _servicio = new CierreServicio(_uot, mermaServicio);
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
    }

    private async Task<Usuario> CrearUsuarioAsync()
    {
        var rol = new Rol("Encargado");
        await _contexto.Set<Rol>().AddAsync(rol);
        var usuario = new Usuario("encargado", null, "$2a$12$hash", "Encargado Test", rol);
        await _contexto.Set<Usuario>().AddAsync(usuario);
        await _contexto.SaveChangesAsync();
        return usuario;
    }

    [Fact]
    public async Task AbrirCierre_PrimerVezDelDia_CreaRegistroAbierto()
    {
        var usuario = await CrearUsuarioAsync();

        var dto = await _servicio.AbrirCierreAsync(usuario.Id);

        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), dto.Fecha);
        Assert.False(dto.EsCerrado);
    }

    [Fact]
    public async Task AbrirCierre_DiaDuplicado_DevuelveExistente()
    {
        var usuario = await CrearUsuarioAsync();

        var primero = await _servicio.AbrirCierreAsync(usuario.Id);
        var segundo = await _servicio.AbrirCierreAsync(usuario.Id);

        Assert.Equal(primero.Id, segundo.Id);
        var count = _contexto.Set<CierreDia>().Count();
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task AbrirCierre_UsuarioInexistente_LanzaExcepcion()
    {
        await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.AbrirCierreAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task CerrarDia_ConDiaAbierto_MarcaCerradoYCalculaDiferencia()
    {
        var usuario = await CrearUsuarioAsync();
        await _servicio.AbrirCierreAsync(usuario.Id);

        var dto = await _servicio.CerrarDiaAsync(new CierreCajaRequest
        {
            EfectivoReal = 500m,
            TarjetaReal = 200m,
            Observacion = "Cierre con diferencia inicial"
        }, usuario.Id);

        Assert.True(dto.EsCerrado);
        Assert.NotNull(dto.CerradoEn);
        Assert.Equal(500m, dto.EfectivoReal);
        Assert.Equal(200m, dto.TarjetaReal);
        Assert.Equal(500m - dto.TotalEfectivo, dto.DiferenciaEfectivo);
    }

    [Fact]
    public async Task ObtenerCierreHoyAsync_ConPagoRegistrado_DebeMostrarTotalesSistemaEnVivo()
    {
        var usuario = await CrearUsuarioAsync();
        await _servicio.AbrirCierreAsync(usuario.Id);

        var mesa = new Mesa(20, 4);
        var categoria = new CategoriaProducto("Bebidas cierre");
        var producto = new Producto("Agua mineral cierre", 2.50m, categoria);
        var pedido = new Pedido(TipoServicio.ComerAqui, mesa);
        pedido.AgregarDetalle(new DetallePedido(producto, 1, 2.50m));
        pedido.MarcarEnPreparacion();
        pedido.MarcarEnCobro();
        var cuenta = pedido.CrearCuentas(1).Single();
        cuenta.Pagar(MetodoPago.Efectivo, usuarioId: usuario.Id);
        pedido.MarcarComoPagado();

        await _contexto.Set<Mesa>().AddAsync(mesa);
        await _contexto.Set<CategoriaProducto>().AddAsync(categoria);
        await _contexto.Set<Producto>().AddAsync(producto);
        await _contexto.Set<Pedido>().AddAsync(pedido);
        await _contexto.Set<Pago>().AddAsync(new Pago(cuenta.Id, cuenta.Total, MetodoPago.Efectivo, usuarioId: usuario.Id));
        await _contexto.SaveChangesAsync();

        var dto = await _servicio.ObtenerCierreHoyAsync();

        Assert.NotNull(dto);
        Assert.False(dto!.EsCerrado);
        Assert.Equal(2.50m, dto.TotalVentas);
        Assert.Equal(2.50m, dto.TotalEfectivo);
        Assert.Equal(0m, dto.TotalTarjeta);
        Assert.Equal(1, dto.TotalPedidos);
    }

    [Fact]
    public async Task CerrarDia_ConTurnoCajaActivo_DebeRechazarCierre()
    {
        var usuario = await CrearUsuarioAsync();
        await _servicio.AbrirCierreAsync(usuario.Id);
        await _contexto.Set<TurnoCaja>().AddAsync(new TurnoCaja(usuario.Id, 100m));
        await _contexto.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.CerrarDiaAsync(new CierreCajaRequest { EfectivoReal = 0m, TarjetaReal = 0m }, usuario.Id));

        Assert.Contains("turno de caja activo", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CerrarDia_SinDiaAbierto_LanzaExcepcion()
    {
        var usuario = await CrearUsuarioAsync();

        await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.CerrarDiaAsync(new CierreCajaRequest { EfectivoReal = 0, TarjetaReal = 0 }, usuario.Id));
    }

    [Fact]
    public async Task CerrarDia_DiaCerrado_NoPermiteVolverACerrar()
    {
        var usuario = await CrearUsuarioAsync();
        await _servicio.AbrirCierreAsync(usuario.Id);
        await _servicio.CerrarDiaAsync(new CierreCajaRequest { EfectivoReal = 0, TarjetaReal = 0 }, usuario.Id);

        await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.CerrarDiaAsync(new CierreCajaRequest { EfectivoReal = 0, TarjetaReal = 0 }, usuario.Id));
    }

    [Fact]
    public async Task CerrarDia_ActualizaRegistroExistente_NoInsertaNuevo()
    {
        var usuario = await CrearUsuarioAsync();
        await _servicio.AbrirCierreAsync(usuario.Id);
        await _servicio.CerrarDiaAsync(new CierreCajaRequest { EfectivoReal = 100m, TarjetaReal = 50m, Observacion = "Cierre test" }, usuario.Id);

        var count = _contexto.Set<CierreDia>().Count();
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task CierreDia_Cerrar_CalculaDiferenciaNegativaCuandoFaltaEfectivo()
    {
        var usuario = await CrearUsuarioAsync();
        var cierre = new CierreDia(DateOnly.FromDateTime(DateTime.UtcNow), 1000m, 600m, 400m, 5, 0, 0m, usuario);
        await _contexto.Set<CierreDia>().AddAsync(cierre);
        await _contexto.SaveChangesAsync();

        cierre.Cerrar(1000m, 600m, 400m, 5, 0, 0m, efectivoReal: 550m, tarjetaReal: 400m, observacion: "Faltante de efectivo");

        Assert.True(cierre.EsCerrado);
        Assert.Equal(-50m, cierre.DiferenciaEfectivo);
        Assert.Equal(0m, cierre.DiferenciaTarjeta);
    }
}
