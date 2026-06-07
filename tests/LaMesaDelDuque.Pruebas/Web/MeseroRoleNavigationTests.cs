using System.Security.Claims;
using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging.Abstractions;
using HomeIndexModel = LaMesaDelDuque.Web.Pages.IndexModel;
using MapaSalonModel = LaMesaDelDuque.Web.Pages.Operaciones.Salon.MapaModel;
using MesasIndexModel = LaMesaDelDuque.Web.Pages.Operaciones.Mesas.IndexModel;

namespace LaMesaDelDuque.Pruebas.Web;

public class MeseroRoleNavigationTests
{
    [Fact]
    public void Home_NoDebeMostrarMesasDeGestionAMesero()
    {
        var page = CreateHomePageForRole("Mesero");

        page.OnGet();

        Assert.DoesNotContain(
            page.ModuleLinks,
            module => module.Label == "Mesas" || module.Page == "/Operaciones/Mesas/Index");
    }

    [Fact]
    public void Home_DebeMostrarMapaSalonYHandoffAMesero()
    {
        var page = CreateHomePageForRole("Mesero");

        page.OnGet();

        Assert.Contains(
            page.ModuleLinks,
            module => module.Label == "Mapa Salón" && module.Page == "/Operaciones/Salon/Mapa");
        Assert.Contains(
            page.ModuleLinks,
            module => module.Label == "Transferir mesas" && module.Page == "/Operaciones/Mesero/Handoff");
    }

    [Fact]
    public void MesasGestion_DebeSeguirExcluyendoMesero()
    {
        var roles = AuthorizeRoles<MesasIndexModel>();

        Assert.Contains("Administrador", roles);
        Assert.Contains("Encargado", roles);
        Assert.DoesNotContain("Mesero", roles);
    }

    [Fact]
    public void MapaSalon_DebePermitirMeseroComoSoloLectura()
    {
        var roles = AuthorizeRoles<MapaSalonModel>();

        Assert.Contains("Administrador", roles);
        Assert.Contains("Encargado", roles);
        Assert.Contains("Mesero", roles);
        Assert.DoesNotContain("Cajero", roles);
    }

    [Fact]
    public async Task MapaSalon_OnGetAsync_ComoMesero_DebeCargarSoloLectura()
    {
        var page = CreateMapaPageForRole("Mesero");

        await page.OnGetAsync();

        Assert.False(page.Vm.PuedeEditar);
        Assert.Single(page.Vm.Mesas);
        Assert.Single(page.Vm.Zonas);
    }

    [Fact]
    public async Task MapaSalon_OnPostActualizarPosicion_ComoMesero_DebeRechazarEdicion()
    {
        var page = CreateMapaPageForRole("Mesero");

        var result = await page.OnPostActualizarPosicionAsync(new MapaSalonModel.ActualizarPosicionRequest
        {
            MesaId = Guid.NewGuid(),
            PosicionX = 10,
            PosicionY = 20,
            ZonaId = Guid.NewGuid(),
            Forma = "Redonda"
        });

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(403, json.StatusCode);
    }

    [Fact]
    public async Task MapaSalon_OnPostCambiarEstado_ComoMesero_DebeRechazarEdicion()
    {
        var page = CreateMapaPageForRole("Mesero");

        var result = await page.OnPostCambiarEstadoAsync(new MapaSalonModel.CambiarEstadoRequest
        {
            MesaId = Guid.NewGuid(),
            NuevoEstado = "Disponible"
        });

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(403, json.StatusCode);
    }

    private static HomeIndexModel CreateHomePageForRole(string rol)
    {
        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, rol.ToLowerInvariant()),
                new Claim(ClaimTypes.Role, rol)
            ],
            authenticationType: "TestAuth");

        return new HomeIndexModel
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

    private static MapaSalonModel CreateMapaPageForRole(string rol)
    {
        var zonaId = Guid.NewGuid();
        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, rol.ToLowerInvariant()),
                new Claim(ClaimTypes.Role, rol)
            ],
            authenticationType: "TestAuth");

        return new MapaSalonModel(
            new FakeMesasServicio(zonaId),
            new FakeZonasSalonServicio(zonaId),
            NullLogger<MapaSalonModel>.Instance)
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

    private static string[] AuthorizeRoles<T>()
    {
        var attribute = typeof(T)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .OfType<AuthorizeAttribute>()
            .Single();

        return attribute.Roles?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [];
    }

    private sealed class FakeMesasServicio(Guid zonaId) : IMesasServicio
    {
        public Task<List<MesaDto>> ListarMesasAsync(CancellationToken cancelacion = default) =>
            Task.FromResult(new List<MesaDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Numero = 5,
                    Capacidad = 4,
                    Estado = "Ocupada",
                    Activa = true,
                    PosicionX = 25,
                    PosicionY = 40,
                    ZonaId = zonaId,
                    Forma = "Redonda"
                }
            });

        public Task<MesaDto?> ObtenerMesaPorNumeroAsync(int numero, CancellationToken cancelacion = default) =>
            Task.FromResult<MesaDto?>(null);

        public Task<MesaDto> CrearMesaAsync(int numero, int capacidad, CancellationToken cancelacion = default) =>
            Task.FromResult(new MesaDto { Id = Guid.NewGuid(), Numero = numero, Capacidad = capacidad });

        public Task<MesaDto> ActualizarMesaAsync(Guid mesaId, int numero, int capacidad, CancellationToken cancelacion = default) =>
            Task.FromResult(new MesaDto { Id = mesaId, Numero = numero, Capacidad = capacidad });

        public Task CambiarEstadoMesaAsync(Guid mesaId, string nuevoEstado, CancellationToken cancelacion = default) =>
            Task.CompletedTask;

        public Task DesactivarMesaAsync(Guid mesaId, CancellationToken cancelacion = default) =>
            Task.CompletedTask;

        public Task<MesaDto> ActualizarPosicionAsync(Guid mesaId, int posicionX, int posicionY, Guid zonaId, string forma, int? rotacion = null, CancellationToken cancelacion = default) =>
            Task.FromResult(new MesaDto { Id = mesaId, PosicionX = posicionX, PosicionY = posicionY, ZonaId = zonaId, Forma = forma, Rotacion = rotacion });

        public Task<MesaDto> LimpiarPosicionAsync(Guid mesaId, CancellationToken cancelacion = default) =>
            Task.FromResult(new MesaDto { Id = mesaId });
    }

    private sealed class FakeZonasSalonServicio(Guid zonaId) : IZonasSalonServicio
    {
        public Task<List<ZonaSalonDto>> ListarActivasAsync(CancellationToken cancelacion = default) =>
            Task.FromResult(new List<ZonaSalonDto>
            {
                new() { Id = zonaId, Nombre = "Salón", Orden = 1, Activa = true }
            });

        public Task<List<ZonaSalonDto>> ListarTodasAsync(CancellationToken cancelacion = default) =>
            ListarActivasAsync(cancelacion);

        public Task<ZonaSalonDto> CrearAsync(string nombre, int orden, CancellationToken cancelacion = default) =>
            Task.FromResult(new ZonaSalonDto { Id = Guid.NewGuid(), Nombre = nombre, Orden = orden, Activa = true });

        public Task<ZonaSalonDto> ActualizarAsync(Guid id, string nombre, int orden, CancellationToken cancelacion = default) =>
            Task.FromResult(new ZonaSalonDto { Id = id, Nombre = nombre, Orden = orden, Activa = true });

        public Task DesactivarAsync(Guid id, CancellationToken cancelacion = default) =>
            Task.CompletedTask;

        public Task ActivarAsync(Guid id, CancellationToken cancelacion = default) =>
            Task.CompletedTask;
    }
}
