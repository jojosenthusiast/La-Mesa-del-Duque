using System.Security.Claims;
using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging.Abstractions;
using HandoffModel = LaMesaDelDuque.Web.Pages.Operaciones.Mesero.HandoffModel;

namespace LaMesaDelDuque.Pruebas.Web;

public class MeseroHandoffPageTests
{
    [Fact]
    public void HandoffPage_DebePermitirGestionYMesero()
    {
        var attribute = typeof(HandoffModel)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .OfType<AuthorizeAttribute>()
            .Single();

        var roles = attribute.Roles?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [];

        Assert.Contains("Administrador", roles);
        Assert.Contains("Encargado", roles);
        Assert.Contains("Mesero", roles);
        Assert.DoesNotContain("Cajero", roles);
    }

    [Fact]
    public async Task OnGetAsync_DebeCargarMesasDelUsuarioActualYMeserosActivos()
    {
        var usuarioId = Guid.NewGuid();
        var shift = new FakeHandoffServicio();
        var usuarios = new FakeHandoffUsuariosServicio
        {
            Usuarios =
            [
                new UsuarioDto { Id = usuarioId, Username = "ana", NombreCompleto = "Ana", RolNombre = "Mesero", Activo = true },
                new UsuarioDto { Id = Guid.NewGuid(), Username = "bob", NombreCompleto = "Bob", RolNombre = "Mesero", Activo = true },
                new UsuarioDto { Id = Guid.NewGuid(), Username = "old", NombreCompleto = "Old", RolNombre = "Mesero", Activo = false },
                new UsuarioDto { Id = Guid.NewGuid(), Username = "admin", NombreCompleto = "Admin", RolNombre = "Administrador", Activo = true }
            ]
        };
        var page = CreatePage(shift, usuarios, usuarioId, "Mesero");

        await page.OnGetAsync();

        Assert.Equal(usuarioId, shift.LastObtenerMesasUsuarioId);
        Assert.Single(page.MesasActivas);
        Assert.Equal(2, page.MeserosActivos.Count);
        Assert.All(page.MeserosActivos, usuario => Assert.Equal("Mesero", usuario.RolNombre));
    }

    [Fact]
    public async Task OnPostTransferirAsync_DebeTransferirMesaConUsuarioActual()
    {
        var actorId = Guid.NewGuid();
        var mesaId = Guid.NewGuid();
        var nuevoMeseroId = Guid.NewGuid();
        var shift = new FakeHandoffServicio();
        var usuarios = new FakeHandoffUsuariosServicio();
        var page = CreatePage(shift, usuarios, actorId, "Mesero");
        page.MesaId = mesaId;
        page.NuevoMeseroId = nuevoMeseroId;

        var result = await page.OnPostTransferirAsync();

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal(mesaId, shift.LastTransferMesaId);
        Assert.Equal(nuevoMeseroId, shift.LastTransferNuevoMeseroId);
        Assert.Equal(actorId, shift.LastTransferUsuarioResponsableId);
    }

    [Fact]
    public async Task OnPostTransferirAsync_SinUsuarioActual_DebeRechazarSinTransferir()
    {
        var shift = new FakeHandoffServicio();
        var usuarios = new FakeHandoffUsuariosServicio();
        var page = CreatePage(shift, usuarios, null);
        page.MesaId = Guid.NewGuid();
        page.NuevoMeseroId = Guid.NewGuid();

        var result = await page.OnPostTransferirAsync();

        Assert.IsType<PageResult>(result);
        Assert.Equal(0, shift.TransferCalls);
        Assert.Contains("identificar", page.ToastError, StringComparison.OrdinalIgnoreCase);
    }

    private static HandoffModel CreatePage(FakeHandoffServicio shift, FakeHandoffUsuariosServicio usuarios, Guid? usuarioId, params string[] roles)
    {
        var claims = new List<Claim>();
        if (usuarioId.HasValue)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, usuarioId.Value.ToString()));
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var identity = usuarioId.HasValue
            ? new ClaimsIdentity(claims, authenticationType: "TestAuth")
            : new ClaimsIdentity();

        return new HandoffModel(shift, usuarios, NullLogger<HandoffModel>.Instance)
        {
            PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            }
        };
    }

    private sealed class FakeHandoffServicio : IShiftHandoffServicio
    {
        public Guid? LastObtenerMesasUsuarioId { get; private set; }
        public int TransferCalls { get; private set; }
        public Guid? LastTransferMesaId { get; private set; }
        public Guid? LastTransferNuevoMeseroId { get; private set; }
        public Guid? LastTransferUsuarioResponsableId { get; private set; }

        public Task<List<MesaAsignadaDto>> ObtenerMesasActivasAsync(Guid usuarioId, CancellationToken cancelacion = default)
        {
            LastObtenerMesasUsuarioId = usuarioId;
            return Task.FromResult(new List<MesaAsignadaDto>
            {
                new()
                {
                    MesaId = Guid.NewGuid(),
                    PedidoId = Guid.NewGuid(),
                    MeseroAsignadoId = usuarioId,
                    MesaNumero = 5,
                    Capacidad = 4,
                    EstadoPedido = "EnPreparacion",
                    Total = 120m,
                    MinutosOcupada = 35
                }
            });
        }

        public Task TransferirMesaAsync(Guid mesaId, Guid nuevoMeseroId, Guid usuarioResponsableId, CancellationToken cancelacion = default)
        {
            TransferCalls++;
            LastTransferMesaId = mesaId;
            LastTransferNuevoMeseroId = nuevoMeseroId;
            LastTransferUsuarioResponsableId = usuarioResponsableId;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeHandoffUsuariosServicio : IUsuariosServicio
    {
        public List<UsuarioDto> Usuarios { get; set; } = [];

        public Task<UsuarioDto> CrearUsuarioAsync(string username, string? email, string password, string nombreCompleto, Guid rolId, CancellationToken cancelacion = default) =>
            throw new NotImplementedException();

        public Task<List<UsuarioDto>> ListarUsuariosAsync(CancellationToken cancelacion = default) =>
            Task.FromResult(Usuarios);

        public Task<List<RolDto>> ListarRolesAsync(CancellationToken cancelacion = default) =>
            Task.FromResult(new List<RolDto>());

        public Task DesactivarUsuarioAsync(Guid usuarioId, CancellationToken cancelacion = default) =>
            Task.CompletedTask;

        public Task<UsuarioDto?> ValidarCredencialesAsync(string username, string password, CancellationToken cancelacion = default) =>
            Task.FromResult<UsuarioDto?>(null);
    }
}
