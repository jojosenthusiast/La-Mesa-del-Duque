using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Web.Pages.Admin.Usuarios;

namespace LaMesaDelDuque.Pruebas.Web;

public class UsuariosPageTests
{
    [Fact]
    public async Task OnGetAsync_loads_users()
    {
        var servicio = new FakeUsuariosServicio
        {
            Usuarios =
            [
                new UsuarioDto { Id = Guid.NewGuid(), Username = "admin", NombreCompleto = "Admin", RolNombre = "Administrador", Activo = true },
                new UsuarioDto { Id = Guid.NewGuid(), Username = "mesero1", NombreCompleto = "Mesero 1", RolNombre = "Mesero", Activo = true }
            ]
        };

        var page = new IndexModel(servicio);

        await page.OnGetAsync();

        Assert.Equal(2, page.Vm.Usuarios.Count);
        Assert.Equal("admin", page.Vm.Usuarios[0].Username);
    }

    [Fact]
    public async Task OnPostCrearAsync_creates_user_and_sets_toast()
    {
        var servicio = new FakeUsuariosServicio();
        var page = new IndexModel(servicio)
        {
            Vm =
            {
                Form = new()
                {
                    Username = "operador",
                    NombreCompleto = "Operador Uno",
                    Password = "password123",
                    RolId = Guid.NewGuid()
                }
            }
        };

        var result = await page.OnPostCrearAsync();

        Assert.IsType<Microsoft.AspNetCore.Mvc.RedirectToPageResult>(result);
        Assert.True(servicio.CrearInvocado);
    }
}

internal sealed class FakeUsuariosServicio : IUsuariosServicio
{
    public List<UsuarioDto> Usuarios { get; set; } = [];
    public bool CrearInvocado { get; private set; }

    public Task<UsuarioDto> CrearUsuarioAsync(string username, string? email, string password, string nombreCompleto, Guid rolId, CancellationToken cancelacion = default)
    {
        CrearInvocado = true;
        var usuario = new UsuarioDto
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            NombreCompleto = nombreCompleto,
            RolNombre = "Operador",
            Activo = true
        };
        Usuarios.Add(usuario);
        return Task.FromResult(usuario);
    }

    public Task<List<UsuarioDto>> ListarUsuariosAsync(CancellationToken cancelacion = default)
        => Task.FromResult(Usuarios);

    public Task DesactivarUsuarioAsync(Guid usuarioId, CancellationToken cancelacion = default)
        => Task.CompletedTask;

    public Task<UsuarioDto?> ValidarCredencialesAsync(string username, string password, CancellationToken cancelacion = default)
        => Task.FromResult<UsuarioDto?>(null);

    public Task<List<RolDto>> ListarRolesAsync(CancellationToken cancelacion = default)
        => Task.FromResult(new List<RolDto>());
}
