using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Entidades;
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
