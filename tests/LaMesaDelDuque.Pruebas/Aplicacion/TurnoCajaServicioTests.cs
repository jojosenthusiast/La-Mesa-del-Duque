using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using LaMesaDelDuque.Infraestructura.Repositorios;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Pruebas.Aplicacion;

public sealed class TurnoCajaServicioTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly LaMesaDelDuqueDbContext _contexto;
    private readonly IUnidadDeTrabajo _uot;
    private readonly ITurnoCajaServicio _servicio;

    public TurnoCajaServicioTests()
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
            new CierreDiaRepositorio(_contexto),
            new ZonaSalonRepositorio(_contexto));

        _servicio = new TurnoCajaServicio(_uot);
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
    }

    [Fact]
    public async Task AbrirTurnoAsync_SinDiaOperativoAbierto_DebeRechazar()
    {
        var cajero = await CrearCajeroAsync();

        var ex = await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.AbrirTurnoAsync(cajero.Id, 100m));

        Assert.Contains("día operativo", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CerrarTurnoAsync_DebePersistirEstadoCerradoYConteo()
    {
        var cajero = await CrearCajeroAsync();
        await AbrirDiaOperativoAsync(cajero);
        var turno = await _servicio.AbrirTurnoAsync(cajero.Id, 100m);

        await _servicio.CerrarTurnoAsync(turno.Id, 0m, null);
        _contexto.ChangeTracker.Clear();

        var recargado = await _uot.TurnosCaja.ObtenerPorIdAsync(turno.Id);

        Assert.NotNull(recargado);
        Assert.True(recargado!.Cerrado);
        Assert.NotNull(recargado.FechaCierre);
        Assert.Equal(0m, recargado.EfectivoContado);
        Assert.Equal(0m, recargado.Diferencia);
    }

    [Fact]
    public async Task RegistrarMovimientoAsync_DebeAparecerEnReporteZ()
    {
        var cajero = await CrearCajeroAsync();
        await AbrirDiaOperativoAsync(cajero);
        var turno = await _servicio.AbrirTurnoAsync(cajero.Id, 100m);

        await _servicio.RegistrarMovimientoAsync(turno.Id, "retiro_seguridad", 25m, "Retiro a caja fuerte", cajero.Id);
        _contexto.ChangeTracker.Clear();

        var reporte = await _servicio.GenerarReporteZAsync(turno.Id);

        var movimiento = Assert.Single(reporte.Movimientos);
        Assert.Equal("retiro_seguridad", movimiento.Tipo);
        Assert.Equal(25m, movimiento.Monto);
        Assert.Equal(25m, reporte.TotalRetiroSeguridad);
    }

    private async Task<Usuario> CrearCajeroAsync()
    {
        var rol = new Rol("Cajero");
        var usuario = new Usuario("cajero", null, "hash-demo", "Cajero Test", rol);

        _contexto.Set<Rol>().Add(rol);
        _contexto.Set<Usuario>().Add(usuario);
        await _contexto.SaveChangesAsync();

        return usuario;
    }

    private async Task AbrirDiaOperativoAsync(Usuario usuario)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        _contexto.Set<CierreDia>().Add(new CierreDia(hoy, 0, 0, 0, 0, 0, 0, usuario));
        await _contexto.SaveChangesAsync();
    }
}
