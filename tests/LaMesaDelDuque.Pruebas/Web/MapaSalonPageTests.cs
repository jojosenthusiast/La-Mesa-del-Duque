using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Web.Pages.Operaciones.Salon;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;

namespace LaMesaDelDuque.Pruebas.Web;

public class MapaSalonPageTests
{
    [Fact]
    public async Task OnGetAsync_Carga_Zonas_Activas_Y_Mesas_Con_Posicion()
    {
        var mesasServicio = new FakeMapaMesasServicio();
        var zonasServicio = new FakeMapaZonasServicio();

        var page = new MapaModel(mesasServicio, zonasServicio, NullLogger<MapaModel>.Instance);
        SetUserRoles(page, "Encargado");

        await page.OnGetAsync();

        Assert.Equal(2, page.Vm.Zonas.Count);
        Assert.Equal(2, page.Vm.Mesas.Count);
        Assert.Contains(page.Vm.Mesas, m => m.Numero == 1);
        Assert.DoesNotContain(page.Vm.Mesas, m => m.Numero == 3); // mesa sin posición
        Assert.True(page.Vm.PuedeEditar);
    }

    [Fact]
    public async Task OnGetAsync_Mesero_No_Puede_Editar()
    {
        var mesasServicio = new FakeMapaMesasServicio();
        var zonasServicio = new FakeMapaZonasServicio();

        var page = new MapaModel(mesasServicio, zonasServicio, NullLogger<MapaModel>.Instance);
        SetUserRoles(page, "Mesero");

        await page.OnGetAsync();

        Assert.False(page.Vm.PuedeEditar);
    }

    [Fact]
    public async Task OnPostActualizarPosicion_Autorizado_Debe_Retornar_Exito()
    {
        var mesasServicio = new FakeMapaMesasServicio();
        var zonasServicio = new FakeMapaZonasServicio();

        var page = new MapaModel(mesasServicio, zonasServicio, NullLogger<MapaModel>.Instance);
        SetUserRoles(page, "Encargado");

        var request = new MapaModel.ActualizarPosicionRequest
        {
            MesaId = mesasServicio.Mesas[0].Id,
            PosicionX = 60,
            PosicionY = 40,
            ZonaId = Guid.NewGuid(),
            Forma = "Redonda"
        };

        var result = await page.OnPostActualizarPosicionAsync(request);

        var json = Assert.IsType<JsonResult>(result);
        var data = JsonToDict(json.Value);
        Assert.True(data.ContainsKey("exito") && (bool)data["exito"]!);
    }

    [Fact]
    public async Task OnPostActualizarPosicion_NoAutorizado_Debe_Retornar_403()
    {
        var mesasServicio = new FakeMapaMesasServicio();
        var zonasServicio = new FakeMapaZonasServicio();

        var page = new MapaModel(mesasServicio, zonasServicio, NullLogger<MapaModel>.Instance);
        SetUserRoles(page, "Mesero");

        var request = new MapaModel.ActualizarPosicionRequest
        {
            MesaId = Guid.NewGuid(),
            PosicionX = 60,
            PosicionY = 40,
            ZonaId = Guid.NewGuid(),
            Forma = "Redonda"
        };

        var result = await page.OnPostActualizarPosicionAsync(request);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(403, json.StatusCode);
    }

    [Fact]
    public async Task OnPostCambiarEstado_Mesero_Autorizado_Debe_Retornar_Exito()
    {
        var mesasServicio = new FakeMapaMesasServicio();
        var zonasServicio = new FakeMapaZonasServicio();

        var page = new MapaModel(mesasServicio, zonasServicio, NullLogger<MapaModel>.Instance);
        SetUserRoles(page, "Mesero");

        var request = new MapaModel.CambiarEstadoRequest
        {
            MesaId = mesasServicio.Mesas[0].Id,
            NuevoEstado = "Ocupada"
        };

        var result = await page.OnPostCambiarEstadoAsync(request);

        var json = Assert.IsType<JsonResult>(result);
        var data = JsonToDict(json.Value);
        Assert.True(data.ContainsKey("exito") && (bool)data["exito"]!);
    }

    [Fact]
    public async Task OnPostCambiarEstado_NoAutenticado_Debe_Retornar_403()
    {
        var mesasServicio = new FakeMapaMesasServicio();
        var zonasServicio = new FakeMapaZonasServicio();

        var page = new MapaModel(mesasServicio, zonasServicio, NullLogger<MapaModel>.Instance);
        // Sin roles

        var request = new MapaModel.CambiarEstadoRequest
        {
            MesaId = Guid.NewGuid(),
            NuevoEstado = "Ocupada"
        };

        var result = await page.OnPostCambiarEstadoAsync(request);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(403, json.StatusCode);
    }

    [Fact]
    public async Task OnGetObtenerDatos_Retorna_Json_Con_Zonas_Y_Mesas()
    {
        var mesasServicio = new FakeMapaMesasServicio();
        var zonasServicio = new FakeMapaZonasServicio();

        var page = new MapaModel(mesasServicio, zonasServicio, NullLogger<MapaModel>.Instance);
        SetUserRoles(page, "Mesero");

        var result = await page.OnGetObtenerDatosAsync();

        var json = Assert.IsType<JsonResult>(result);
        Assert.NotNull(json.Value);
    }

    private static void SetUserRoles(MapaModel page, params string[] roles)
    {
        var claims = roles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();
        claims.Add(new Claim(ClaimTypes.Name, "testuser"));
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        page.PageContext = new PageContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private static Dictionary<string, object?> JsonToDict(object? value)
    {
        if (value is null) return [];
        var props = value.GetType().GetProperties()
            .Where(p => p.CanRead)
            .ToDictionary(p => p.Name, p => p.GetValue(value));
        return props;
    }
}

internal sealed class FakeMapaMesasServicio : IMesasServicio
{
    public List<MesaDto> Mesas { get; } =
    [
        new() { Id = Guid.NewGuid(), Numero = 1, Capacidad = 4, Estado = "Disponible", Activa = true, PosicionX = 10, PosicionY = 20, ZonaId = Guid.NewGuid(), Forma = "Redonda", Rotacion = 0 },
        new() { Id = Guid.NewGuid(), Numero = 2, Capacidad = 6, Estado = "Ocupada", Activa = true, PosicionX = 50, PosicionY = 50, ZonaId = Guid.NewGuid(), Forma = "Cuadrada", Rotacion = 45 },
        new() { Id = Guid.NewGuid(), Numero = 3, Capacidad = 2, Estado = "Disponible", Activa = true } // sin posición
    ];

    public Task<List<MesaDto>> ListarMesasAsync(CancellationToken cancelacion = default)
        => Task.FromResult(Mesas);

    public Task<MesaDto?> ObtenerMesaPorNumeroAsync(int numero, CancellationToken cancelacion = default)
        => Task.FromResult(Mesas.FirstOrDefault(m => m.Numero == numero));

    public Task<MesaDto> CrearMesaAsync(int numero, int capacidad, CancellationToken cancelacion = default)
        => Task.FromResult(new MesaDto { Id = Guid.NewGuid(), Numero = numero, Capacidad = capacidad, Estado = "Disponible", Activa = true });

    public Task<MesaDto> ActualizarMesaAsync(Guid mesaId, int numero, int capacidad, CancellationToken cancelacion = default)
        => Task.FromResult(new MesaDto { Id = mesaId, Numero = numero, Capacidad = capacidad, Estado = "Disponible", Activa = true });

    public Task CambiarEstadoMesaAsync(Guid mesaId, string nuevoEstado, CancellationToken cancelacion = default)
        => Task.CompletedTask;

    public Task DesactivarMesaAsync(Guid mesaId, CancellationToken cancelacion = default)
        => Task.CompletedTask;

    public Task<MesaDto> ActualizarPosicionAsync(Guid mesaId, int posicionX, int posicionY, Guid zonaId, string forma, int? rotacion = null, CancellationToken cancelacion = default)
        => Task.FromResult(new MesaDto { Id = mesaId, Numero = 1, Capacidad = 4, Estado = "Disponible", Activa = true, PosicionX = posicionX, PosicionY = posicionY, ZonaId = zonaId, Forma = forma, Rotacion = rotacion ?? 0 });

    public Task<MesaDto> LimpiarPosicionAsync(Guid mesaId, CancellationToken cancelacion = default)
        => Task.FromResult(new MesaDto { Id = mesaId, Numero = 1, Capacidad = 4, Estado = "Disponible", Activa = true });
}

internal sealed class FakeMapaZonasServicio : IZonasSalonServicio
{
    public Task<List<ZonaSalonDto>> ListarActivasAsync(CancellationToken cancelacion = default)
        => Task.FromResult(new List<ZonaSalonDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Terraza", Orden = 1, Activa = true },
            new() { Id = Guid.NewGuid(), Nombre = "Interior", Orden = 2, Activa = true }
        });

    public Task<List<ZonaSalonDto>> ListarTodasAsync(CancellationToken cancelacion = default)
        => Task.FromResult(new List<ZonaSalonDto>());

    public Task<ZonaSalonDto> CrearAsync(string nombre, int orden, CancellationToken cancelacion = default)
        => Task.FromResult(new ZonaSalonDto { Id = Guid.NewGuid(), Nombre = nombre, Orden = orden, Activa = true });

    public Task<ZonaSalonDto> ActualizarAsync(Guid id, string nombre, int orden, CancellationToken cancelacion = default)
        => Task.FromResult(new ZonaSalonDto { Id = id, Nombre = nombre, Orden = orden, Activa = true });

    public Task DesactivarAsync(Guid id, CancellationToken cancelacion = default)
        => Task.CompletedTask;

    public Task ActivarAsync(Guid id, CancellationToken cancelacion = default)
        => Task.CompletedTask;
}
