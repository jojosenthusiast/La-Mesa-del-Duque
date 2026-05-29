using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using LaMesaDelDuque.Infraestructura.Repositorios;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace LaMesaDelDuque.Pruebas.Aplicacion;

public class ShiftHandoffServicioTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly LaMesaDelDuqueDbContext _contexto;
    private readonly IUnidadDeTrabajo _uot;
    private readonly IShiftHandoffServicio _servicio;

    public ShiftHandoffServicioTests()
    {
        _conexion = new SqliteConnection("Data Source=:memory:");
        _conexion.Open();

        var opciones = new DbContextOptionsBuilder<LaMesaDelDuqueDbContext>()
            .UseSqlite(_conexion)
            .Options;

        _contexto = new LaMesaDelDuqueDbContext(opciones);
        _contexto.Database.EnsureCreated();
        _uot = CrearUnidadDeTrabajo(_contexto);
        _servicio = new ShiftHandoffServicio(_uot, NullLogger<ShiftHandoffServicio>.Instance);
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
    }

    [Fact]
    public async Task ObtenerMesasActivas_DebeFiltrarPorMeseroAsignado()
    {
        var (ana, bob, _, producto) = await SembrarUsuariosYProductoAsync();
        var mesaAna = await CrearPedidoMesaAsync(1, ana, producto);
        await CrearPedidoMesaAsync(2, bob, producto);

        var mesas = await _servicio.ObtenerMesasActivasAsync(ana.Id);

        var asignada = Assert.Single(mesas);
        Assert.Equal(mesaAna.Id, asignada.MesaId);
        Assert.Equal(1, asignada.MesaNumero);
        Assert.Equal(ana.Id, asignada.MeseroAsignadoId);
        Assert.NotEqual(Guid.Empty, asignada.PedidoId);
        Assert.Equal("EnPreparacion", asignada.EstadoPedido);
    }

    [Fact]
    public async Task TransferirMesa_DebeCambiarMeseroDelPedidoActivoYAuditar()
    {
        var (ana, bob, encargado, producto) = await SembrarUsuariosYProductoAsync();
        var mesa = await CrearPedidoMesaAsync(3, ana, producto);

        await _servicio.TransferirMesaAsync(mesa.Id, bob.Id, encargado.Id);

        var pedido = await _contexto.Set<Pedido>()
            .AsNoTracking()
            .SingleAsync(p => p.Mesa != null && p.Mesa.Id == mesa.Id);
        Assert.Equal(bob.Id, pedido.MeseroAsignadoId);

        var auditoria = await _contexto.Set<Auditoria>().SingleAsync();
        Assert.Equal("pedido", auditoria.TablaAfectada);
        Assert.Equal(pedido.Id, auditoria.RegistroId);
        Assert.Equal("UPDATE", auditoria.Accion);
        Assert.Equal(encargado.Id, auditoria.UsuarioId);
        Assert.Contains(ana.Id.ToString(), auditoria.DatosAnteriores);
        Assert.Contains(bob.Id.ToString(), auditoria.DatosNuevos);
    }

    [Fact]
    public async Task TransferirMesa_UsuarioDestinoNoMesero_DebeRechazar()
    {
        var (ana, _, encargado, producto) = await SembrarUsuariosYProductoAsync();
        var mesa = await CrearPedidoMesaAsync(4, ana, producto);

        var excepcion = await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.TransferirMesaAsync(mesa.Id, encargado.Id, encargado.Id));

        Assert.Contains("Mesero", excepcion.Message, StringComparison.OrdinalIgnoreCase);
    }


    [Fact]
    public async Task TransferirMesa_AlMismoMesero_DebeRechazarSinAuditar()
    {
        var (ana, _, encargado, producto) = await SembrarUsuariosYProductoAsync();
        var mesa = await CrearPedidoMesaAsync(5, ana, producto);

        var excepcion = await Assert.ThrowsAsync<ReglaDominioException>(() =>
            _servicio.TransferirMesaAsync(mesa.Id, ana.Id, encargado.Id));

        Assert.Contains("ya", excepcion.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(await _contexto.Set<Auditoria>().ToListAsync());
    }

    private async Task<(Usuario Ana, Usuario Bob, Usuario Encargado, Producto Producto)> SembrarUsuariosYProductoAsync()
    {
        var rolMesero = new Rol("Mesero", "Atiende mesas");
        var rolEncargado = new Rol("Encargado", "Gestiona turno");
        var ana = new Usuario("ana", "ana@lmd.test", "hash", "Ana Mesero", rolMesero);
        var bob = new Usuario("bob", "bob@lmd.test", "hash", "Bob Mesero", rolMesero);
        var encargado = new Usuario("encargado", "encargado@lmd.test", "hash", "Encargado", rolEncargado);
        var categoria = new CategoriaProducto("Platos fuertes");
        var producto = new Producto("Tosta", 10m, categoria);

        _contexto.Set<Rol>().AddRange(rolMesero, rolEncargado);
        _contexto.Set<Usuario>().AddRange(ana, bob, encargado);
        _contexto.Set<CategoriaProducto>().Add(categoria);
        _contexto.Set<Producto>().Add(producto);
        await _contexto.SaveChangesAsync();

        return (ana, bob, encargado, producto);
    }

    private async Task<Mesa> CrearPedidoMesaAsync(int numeroMesa, Usuario mesero, Producto producto)
    {
        var mesa = new Mesa(numeroMesa, 4);
        mesa.Ocupar();

        var pedido = new Pedido(TipoServicio.ComerAqui, mesa);
        pedido.AsignarMesero(mesero.Id);
        pedido.AgregarDetalle(new DetallePedido(producto, 1, producto.Precio));
        pedido.MarcarEnPreparacion();

        _contexto.Set<Mesa>().Add(mesa);
        _contexto.Set<Pedido>().Add(pedido);
        await _contexto.SaveChangesAsync();
        return mesa;
    }

    private static IUnidadDeTrabajo CrearUnidadDeTrabajo(LaMesaDelDuqueDbContext contexto) =>
        new UnidadDeTrabajo(contexto,
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
