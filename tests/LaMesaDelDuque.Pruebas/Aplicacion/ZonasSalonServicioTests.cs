using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using LaMesaDelDuque.Infraestructura.Repositorios;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Pruebas.Aplicacion;

public class ZonasSalonServicioTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly LaMesaDelDuqueDbContext _contexto;
    private readonly IZonasSalonServicio _servicio;
    private readonly IUnidadDeTrabajo _uot;

    public ZonasSalonServicioTests()
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

        _servicio = new ZonasSalonServicio(_uot);
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
    }

    [Fact]
    public async Task ListarActivas_SinZonas_DebeRetornarListaVacia()
    {
        var zonas = await _servicio.ListarActivasAsync();

        Assert.NotNull(zonas);
        Assert.Empty(zonas);
    }

    [Fact]
    public async Task CrearZona_ConDatosValidos_DebeCrearYRetornarDto()
    {
        var dto = await _servicio.CrearAsync("Terraza", 1);

        Assert.NotNull(dto);
        Assert.NotEqual(Guid.Empty, dto.Id);
        Assert.Equal("Terraza", dto.Nombre);
        Assert.Equal(1, dto.Orden);
        Assert.True(dto.Activa);
    }

    [Fact]
    public async Task ListarActivas_ConZonas_ActivasYInactivas_DebeRetornarSoloActivas()
    {
        await _servicio.CrearAsync("Terraza", 1);
        await _servicio.CrearAsync("Bar", 2);
        var interior = await _servicio.CrearAsync("Interior", 0);
        await _servicio.DesactivarAsync(interior.Id);

        var zonas = await _servicio.ListarActivasAsync();

        Assert.Equal(2, zonas.Count);
        Assert.DoesNotContain(zonas, z => z.Nombre == "Interior");
    }

    [Fact]
    public async Task ListarActivas_DebeEstarOrdenadaPorOrden()
    {
        await _servicio.CrearAsync("Bar", 2);
        await _servicio.CrearAsync("Terraza", 1);
        await _servicio.CrearAsync("Interior", 0);

        var zonas = await _servicio.ListarActivasAsync();

        Assert.Equal("Interior", zonas[0].Nombre);
        Assert.Equal("Terraza", zonas[1].Nombre);
        Assert.Equal("Bar", zonas[2].Nombre);
    }

    [Fact]
    public async Task ListarTodas_DebeIncluirInactivas()
    {
        await _servicio.CrearAsync("Terraza", 1);
        var bar = await _servicio.CrearAsync("Bar", 2);
        await _servicio.DesactivarAsync(bar.Id);

        var zonas = await _servicio.ListarTodasAsync();

        Assert.Equal(2, zonas.Count);
    }

    [Fact]
    public async Task ActualizarZona_CuandoExiste_DebeActualizar()
    {
        var zona = await _servicio.CrearAsync("Terraza", 1);

        var actualizada = await _servicio.ActualizarAsync(zona.Id, "Patio", 3);

        Assert.Equal("Patio", actualizada.Nombre);
        Assert.Equal(3, actualizada.Orden);
    }

    [Fact]
    public async Task ActualizarZona_Inexistente_DebeLanzarExcepcion()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.ActualizarAsync(Guid.NewGuid(), "Patio", 1));
    }

    [Fact]
    public async Task DesactivarZona_CuandoExiste_DebeDesactivar()
    {
        var zona = await _servicio.CrearAsync("Terraza", 1);

        await _servicio.DesactivarAsync(zona.Id);

        var zonas = await _servicio.ListarActivasAsync();
        Assert.DoesNotContain(zonas, z => z.Id == zona.Id);
    }

    [Fact]
    public async Task ActivarZona_CuandoEstaInactiva_DebeActivar()
    {
        var zona = await _servicio.CrearAsync("Terraza", 1);
        await _servicio.DesactivarAsync(zona.Id);

        await _servicio.ActivarAsync(zona.Id);

        var zonas = await _servicio.ListarActivasAsync();
        Assert.Contains(zonas, z => z.Id == zona.Id);
    }

    [Fact]
    public async Task DesactivarZona_Inexistente_DebeLanzarExcepcion()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.DesactivarAsync(Guid.NewGuid()));
    }

}
