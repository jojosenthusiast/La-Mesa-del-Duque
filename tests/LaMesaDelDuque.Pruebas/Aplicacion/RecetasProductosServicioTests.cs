using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using LaMesaDelDuque.Infraestructura.Repositorios;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Pruebas.Aplicacion;

public class RecetasProductosServicioTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly LaMesaDelDuqueDbContext _contexto;
    private readonly IRecetasProductosServicio _servicio;

    public RecetasProductosServicioTests()
    {
        _conexion = new SqliteConnection("Data Source=:memory:");
        _conexion.Open();

        var opciones = new DbContextOptionsBuilder<LaMesaDelDuqueDbContext>()
            .UseSqlite(_conexion)
            .Options;

        _contexto = new LaMesaDelDuqueDbContext(opciones);
        _contexto.Database.EnsureCreated();

        var uot = new UnidadDeTrabajo(_contexto,
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
            new CuentaRepositorio(_contexto));

        _servicio = new RecetasProductosServicio(uot);
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
    }

    [Fact]
    public async Task AsociarReceta_AProductoExistente_DebePersistirYRetornarDto()
    {
        var categoria = new CategoriaProducto("Platos fuertes");
        var producto = new Producto("Hamburguesa clásica", 6.99m, categoria, tiempoPreparacionMin: 12);
        var pan = new Ingrediente("Pan brioche", "unidad", 20, 5, 0.35m);
        var carne = new Ingrediente("Carne 120g", "unidad", 20, 5, 1.25m);

        _contexto.Set<CategoriaProducto>().Add(categoria);
        _contexto.Set<Producto>().Add(producto);
        _contexto.Set<Ingrediente>().AddRange(pan, carne);
        await _contexto.SaveChangesAsync();

        var dto = await _servicio.CrearRecetaAsync(producto.Id, "Armar, sellar y servir caliente.",
        [
            new RecetaIngredienteCreacionDto(pan.Id, 1),
            new RecetaIngredienteCreacionDto(carne.Id, 1)
        ]);

        Assert.Equal(producto.Id, dto.ProductoId);
        Assert.Equal(2, dto.Ingredientes.Count);
    }

    [Fact]
    public async Task ObtenerReceta_DeProductoSinReceta_DebeRetornarNulo()
    {
        var categoria = new CategoriaProducto("Platos fuertes");
        var producto = new Producto("Hamburguesa clásica", 6.99m, categoria, tiempoPreparacionMin: 12);

        _contexto.Set<CategoriaProducto>().Add(categoria);
        _contexto.Set<Producto>().Add(producto);
        await _contexto.SaveChangesAsync();

        var receta = await _servicio.ObtenerPorProductoIdAsync(producto.Id);

        Assert.Null(receta);
    }
}
