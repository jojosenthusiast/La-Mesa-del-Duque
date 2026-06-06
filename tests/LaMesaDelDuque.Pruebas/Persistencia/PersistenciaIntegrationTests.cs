using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Infraestructura.Persistencia;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Pruebas.Persistencia;

/// <summary>
/// Pruebas de integración de persistencia con SQLite en memoria.
/// Verifica el mapeo de las cinco entidades del Sprint 1:
/// CategoriaProducto, Producto, Mesa, Pedido, DetallePedido.
/// </summary>
public class PersistenciaIntegrationTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly LaMesaDelDuqueDbContext _contexto;

    public PersistenciaIntegrationTests()
    {
        _conexion = new SqliteConnection("Data Source=:memory:");
        _conexion.Open();

        var opciones = new DbContextOptionsBuilder<LaMesaDelDuqueDbContext>()
            .UseSqlite(_conexion)
            .Options;

        _contexto = new LaMesaDelDuqueDbContext(opciones);
        _contexto.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
    }

    [Fact]
    public void CategoriaProducto_AgregarYRecuperar_DebePersistirCorrectamente()
    {
        var categoria = new CategoriaProducto("Bebidas");
        _contexto.Set<CategoriaProducto>().Add(categoria);
        _contexto.SaveChanges();

        _contexto.ChangeTracker.Clear();

        var recuperada = _contexto.Set<CategoriaProducto>().Find(categoria.Id);
        Assert.NotNull(recuperada);
        Assert.Equal("Bebidas", recuperada!.Nombre);
        Assert.True(recuperada.Activo);
    }

    [Fact]
    public void Producto_AgregarYRecuperar_DebePersistirConCategoria()
    {
        var categoria = new CategoriaProducto("Entradas");
        _contexto.Set<CategoriaProducto>().Add(categoria);
        _contexto.SaveChanges();

        var producto = new Producto("Bruschetta", 8.00m, categoria);
        _contexto.Set<Producto>().Add(producto);
        _contexto.SaveChanges();

        _contexto.ChangeTracker.Clear();

        var recuperado = _contexto.Set<Producto>()
            .Include(p => p.Categoria)
            .First(p => p.Id == producto.Id);

        Assert.Equal("Bruschetta", recuperado.Nombre);
        Assert.Equal(8.00m, recuperado.Precio);
        Assert.NotNull(recuperado.Categoria);
        Assert.Equal("Entradas", recuperado.Categoria.Nombre);
    }

    [Fact]
    public void Mesa_AgregarYRecuperar_DebePersistirConEstado()
    {
        var mesa = new Mesa(5, 6);
        _contexto.Set<Mesa>().Add(mesa);
        _contexto.SaveChanges();

        _contexto.ChangeTracker.Clear();

        var recuperada = _contexto.Set<Mesa>().Find(mesa.Id);
        Assert.NotNull(recuperada);
        Assert.Equal(5, recuperada!.Numero);
        Assert.Equal(6, recuperada.Capacidad);
        Assert.Equal(EstadoMesa.Disponible, recuperada.Estado);
    }

    [Fact]
    public void Pedido_AgregarYRecuperar_DebePersistirConMesaYDetalles()
    {
        var mesa = new Mesa(3, 4);
        var categoria = new CategoriaProducto("Bebidas");
        var producto = new Producto("Café Americano", 3.50m, categoria);

        _contexto.Set<Mesa>().Add(mesa);
        _contexto.Set<CategoriaProducto>().Add(categoria);
        _contexto.Set<Producto>().Add(producto);
        _contexto.SaveChanges();

        var pedido = new Pedido(TipoServicio.ComerAqui, mesa);
        pedido.AgregarDetalle(new DetallePedido(producto, 2, 3.50m));
        _contexto.Set<Pedido>().Add(pedido);
        _contexto.SaveChanges();

        _contexto.ChangeTracker.Clear();

        var recuperado = _contexto.Set<Pedido>()
            .Include(p => p.Mesa)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
            .First(p => p.Id == pedido.Id);

        Assert.NotNull(recuperado);
        Assert.Equal(EstadoPedido.Pendiente, recuperado.Estado);
        Assert.NotNull(recuperado.Mesa);
        Assert.Equal(3, recuperado.Mesa.Numero);
        Assert.Single(recuperado.Detalles);
        Assert.Equal(2, recuperado.Detalles[0].Cantidad);
        Assert.Equal(3.50m, recuperado.Detalles[0].PrecioUnitario);
        Assert.NotNull(recuperado.Detalles[0].Producto);
        Assert.Equal("Café Americano", recuperado.Detalles[0].Producto.Nombre);
    }

    [Fact]
    public void Pedido_Total_DebeSerIgnoradoPorEfCore()
    {
        var mesa = new Mesa(1, 2);
        var categoria = new CategoriaProducto("Postres");
        var producto = new Producto("Tiramisú", 7.00m, categoria);

        _contexto.Set<Mesa>().Add(mesa);
        _contexto.Set<CategoriaProducto>().Add(categoria);
        _contexto.Set<Producto>().Add(producto);
        _contexto.SaveChanges();

        var pedido = new Pedido(TipoServicio.ComerAqui, mesa);
        pedido.AgregarDetalle(new DetallePedido(producto, 2, 7.00m));
        Assert.Equal(14.00m, pedido.Total);

        _contexto.Set<Pedido>().Add(pedido);
        _contexto.SaveChanges();

        _contexto.ChangeTracker.Clear();

        var recuperado = _contexto.Set<Pedido>()
            .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
            .First(p => p.Id == pedido.Id);

        // El Total se recalcula desde los detalles recuperados
        Assert.Equal(14.00m, recuperado.Total);
    }

    [Fact]
    public void DetallePedido_DebeRecibirIdGeneradoPorEfCore()
    {
        var mesa = new Mesa(2, 4);
        var categoria = new CategoriaProducto("Carnes");
        var producto = new Producto("Filete", 25.00m, categoria);

        _contexto.Set<Mesa>().Add(mesa);
        _contexto.Set<CategoriaProducto>().Add(categoria);
        _contexto.Set<Producto>().Add(producto);
        _contexto.SaveChanges();

        var pedido = new Pedido(TipoServicio.ComerAqui, mesa);
        var detalle = new DetallePedido(producto, 1, 25.00m);
        pedido.AgregarDetalle(detalle);
        _contexto.Set<Pedido>().Add(pedido);
        _contexto.SaveChanges();

        Assert.NotEqual(Guid.Empty, detalle.Id);
    }

    [Fact]
    public void Mesa_CambiarEstado_DebePersistir()
    {
        var mesa = new Mesa(10, 8);
        _contexto.Set<Mesa>().Add(mesa);
        _contexto.SaveChanges();

        mesa.CambiarEstado(EstadoMesa.Ocupada);
        _contexto.SaveChanges();

        _contexto.ChangeTracker.Clear();

        var recuperada = _contexto.Set<Mesa>().Find(mesa.Id);
        Assert.NotNull(recuperada);
        Assert.Equal(EstadoMesa.Ocupada, recuperada!.Estado);
    }

    [Fact]
    public void CategoriaProducto_Desactivar_DebePersistir()
    {
        var categoria = new CategoriaProducto("Temporales");
        _contexto.Set<CategoriaProducto>().Add(categoria);
        _contexto.SaveChanges();

        categoria.Desactivar();
        _contexto.SaveChanges();

        _contexto.ChangeTracker.Clear();

        var recuperada = _contexto.Set<CategoriaProducto>().Find(categoria.Id);
        Assert.NotNull(recuperada);
        Assert.False(recuperada!.Activo);
    }

    [Fact]
    public void ProveedorEIngrediente_AgregarYRecuperar_DebePersistirConRelacion()
    {
        var proveedor = new Proveedor("Insumos del Norte", "0614-250890-102-3");
        _contexto.Set<Proveedor>().Add(proveedor);
        _contexto.SaveChanges();

        var ingrediente = new Ingrediente("Pan hamburguesa", "unidad", 40m, 10m, 0.35m, proveedor);
        _contexto.Set<Ingrediente>().Add(ingrediente);
        _contexto.SaveChanges();

        _contexto.ChangeTracker.Clear();

        var recuperado = _contexto.Set<Ingrediente>()
            .Include(i => i.ProveedorDefault)
            .First(i => i.Id == ingrediente.Id);

        Assert.Equal("Pan hamburguesa", recuperado.Nombre);
        Assert.Equal("unidad", recuperado.UnidadMedida);
        Assert.NotNull(recuperado.ProveedorDefault);
        Assert.Equal("0614-250890-102-3", recuperado.ProveedorDefault!.Nit);
    }
}
