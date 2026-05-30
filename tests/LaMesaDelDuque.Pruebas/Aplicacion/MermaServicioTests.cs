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

public class MermaServicioTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly LaMesaDelDuqueDbContext _contexto;
    private readonly IUnidadDeTrabajo _uot;
    private readonly IMermaServicio _servicio;

    public MermaServicioTests()
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

        _servicio = new MermaServicio(_uot);
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
    }

    private async Task<(Usuario usuario, Ingrediente ingrediente, CierreDia cierre)> ArrangeAsync()
    {
        var rol = new Rol("Encargado");
        await _contexto.Set<Rol>().AddAsync(rol);

        var usuario = new Usuario("encargado", null, "$2a$12$hash", "Encargado Test", rol);
        await _contexto.Set<Usuario>().AddAsync(usuario);

        var cat = new CategoriaProducto("Bebidas");
        await _contexto.Set<CategoriaProducto>().AddAsync(cat);

        var ingrediente = new Ingrediente("Café", "kg", 10m, 2m, 5m);
        await _contexto.Set<Ingrediente>().AddAsync(ingrediente);

        var cierre = new CierreDia(DateOnly.FromDateTime(DateTime.UtcNow), 0, 0, 0, 0, 0, 0, usuario);
        await _contexto.Set<CierreDia>().AddAsync(cierre);

        await _contexto.SaveChangesAsync();
        return (usuario, ingrediente, cierre);
    }

    [Fact]
    public async Task RegistrarMerma_CuandoDiaAbierto_DebeGuardarYDescontarStock()
    {
        var (usuario, ingrediente, _) = await ArrangeAsync();
        var stockInicial = ingrediente.StockActual;

        var dto = await _servicio.RegistrarMermaAsync(new RegistrarMermaRequest
        {
            IngredienteId = ingrediente.Id,
            Cantidad = 2m,
            Tipo = TipoMerma.DanoManejo,
            Notas = "Bolsa rota"
        }, usuario.Id);

        Assert.Equal(ingrediente.Nombre, dto.IngredienteNombre);
        Assert.Equal(2m, dto.Cantidad);
        Assert.Equal("DanoManejo", dto.Tipo);

        var actualizado = await _contexto.Set<Ingrediente>().FindAsync(ingrediente.Id);
        Assert.Equal(stockInicial - 2m, actualizado!.StockActual);
    }

    [Fact]
    public async Task RegistrarMerma_SinDiaAbierto_DebeLanzarExcepcion()
    {
        var rol = new Rol("Encargado");
        await _contexto.Set<Rol>().AddAsync(rol);
        var usuario = new Usuario("u1", null, "h", "U1", rol);
        await _contexto.Set<Usuario>().AddAsync(usuario);
        var cat = new CategoriaProducto("Cat");
        await _contexto.Set<CategoriaProducto>().AddAsync(cat);
        var ing = new Ingrediente("Sal", "kg", 5m, 1m, 1m);
        await _contexto.Set<Ingrediente>().AddAsync(ing);
        await _contexto.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _servicio.RegistrarMermaAsync(new RegistrarMermaRequest
            {
                IngredienteId = ing.Id,
                Cantidad = 1m
            }, usuario.Id));

        Assert.Contains("cierre", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RegistrarMerma_IngredienteInexistente_DebeLanzarExcepcion()
    {
        var (usuario, _, _) = await ArrangeAsync();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.RegistrarMermaAsync(new RegistrarMermaRequest
            {
                IngredienteId = Guid.NewGuid(),
                Cantidad = 1m
            }, usuario.Id));
    }

    [Fact]
    public async Task ObtenerMermasDelDia_DevuelveSoloLasDeHoy()
    {
        var (usuario, ingrediente, _) = await ArrangeAsync();

        await _servicio.RegistrarMermaAsync(new RegistrarMermaRequest
        {
            IngredienteId = ingrediente.Id,
            Cantidad = 1m
        }, usuario.Id);

        var mermas = await _servicio.ObtenerMermasDelDiaAsync();
        Assert.Single(mermas);
        Assert.Equal(ingrediente.Nombre, mermas[0].IngredienteNombre);
    }
}
