using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Repositorios;
using LaMesaDelDuque.Infraestructura.Persistencia;
using LaMesaDelDuque.Infraestructura.Repositorios;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LaMesaDelDuque.Pruebas.Aplicacion;

public class UsuariosServicioTests : IDisposable
{
    private readonly SqliteConnection _conexion;
    private readonly LaMesaDelDuqueDbContext _contexto;
    private readonly IUsuariosServicio _servicio;
    private readonly IUnidadDeTrabajo _uot;

    public UsuariosServicioTests()
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
            new RecetaProductoRepositorio(_contexto));

        _servicio = new UsuariosServicio(_uot);
    }

    public void Dispose()
    {
        _contexto.Dispose();
        _conexion.Dispose();
    }

    private async Task<Rol> CrearRolAsync(string nombre = "cajero")
    {
        var rol = new Rol(nombre, $"Rol de {nombre}");
        await _uot.Roles.AgregarAsync(rol);
        await _uot.GuardarCambiosAsync();
        return rol;
    }

    [Fact]
    public async Task CrearUsuario_ConDatosValidos_DebeCrearYRetornarDto()
    {
        var rol = await CrearRolAsync();

        var dto = await _servicio.CrearUsuarioAsync("cajero1", "cajero@lmd.test", "password123", "Juan Pérez", rol.Id);

        Assert.Equal("cajero1", dto.Username);
        Assert.Equal("cajero", dto.RolNombre);
        Assert.True(dto.Activo);
    }

    [Fact]
    public async Task CrearUsuario_ConPasswordCorta_DebeLanzarExcepcion()
    {
        var rol = await CrearRolAsync("admin");

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _servicio.CrearUsuarioAsync("admin1", null, "short", "Admin", rol.Id));
    }

    [Fact]
    public async Task CrearUsuario_ConUsernameDuplicado_DebeLanzarExcepcion()
    {
        var rol = await CrearRolAsync("encargado");

        await _servicio.CrearUsuarioAsync("usuario1", null, "password123", "Primer usuario", rol.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _servicio.CrearUsuarioAsync("usuario1", null, "password456", "Segundo usuario", rol.Id));
    }

    [Fact]
    public async Task DesactivarUsuario_DebeDesactivarYPersistir()
    {
        var rol = await CrearRolAsync("cocina");
        var dto = await _servicio.CrearUsuarioAsync("cocinero1", null, "password123", "María López", rol.Id);

        await _servicio.DesactivarUsuarioAsync(dto.Id);

        var usuarios = await _servicio.ListarUsuariosAsync();
        var desactivado = usuarios.First(u => u.Id == dto.Id);
        Assert.False(desactivado.Activo);
    }

    [Fact]
    public async Task ValidarCredenciales_Correctas_DebeRetornarUsuario()
    {
        var rol = await CrearRolAsync("gerente");
        await _servicio.CrearUsuarioAsync("gerente1", null, "password123", "Carlos Ruiz", rol.Id);

        var resultado = await _servicio.ValidarCredencialesAsync("gerente1", "password123");

        Assert.NotNull(resultado);
        Assert.Equal("gerente1", resultado!.Username);
    }

    [Fact]
    public async Task ValidarCredenciales_PasswordIncorrecta_DebeRetornarNulo()
    {
        var rol = await CrearRolAsync("despacho");
        await _servicio.CrearUsuarioAsync("despacho1", null, "password123", "Ana Gómez", rol.Id);

        var resultado = await _servicio.ValidarCredencialesAsync("despacho1", "wrongpassword");

        Assert.Null(resultado);
    }

    [Fact]
    public async Task ValidarCredenciales_UsuarioDesactivado_DebeRetornarNulo()
    {
        var rol = await CrearRolAsync("mesero");
        var dto = await _servicio.CrearUsuarioAsync("mesero1", null, "password123", "Pedro Sánchez", rol.Id);
        await _servicio.DesactivarUsuarioAsync(dto.Id);

        var resultado = await _servicio.ValidarCredencialesAsync("mesero1", "password123");

        Assert.Null(resultado);
    }
}
