using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Web.Pages.Operaciones.Salon;
using LaMesaDelDuque.Web.Models.Operaciones;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;

namespace LaMesaDelDuque.Pruebas.Web;

public class MapaSalonPageTests
{
    [Fact]
    public async Task OnGetAsync_Carga_Zonas_Activas_Y_Todas_Las_Mesas_Del_Catalogo()
    {
        var mesasServicio = new FakeMapaMesasServicio();
        var zonasServicio = new FakeMapaZonasServicio();

        var page = new MapaModel(mesasServicio, zonasServicio, NullLogger<MapaModel>.Instance);
        SetUserRoles(page, "Encargado");

        await page.OnGetAsync();

        Assert.Equal(2, page.Vm.Zonas.Count);
        Assert.Equal(3, page.Vm.TotalMesas);
        Assert.Equal(3, page.Vm.Mesas.Count);
        Assert.Equal(1, page.Vm.MesasPendientesUbicacion);
        Assert.Contains(page.Vm.Mesas, m => m.Numero == 1 && m.EsUbicacionSugerida is false);

        var mesaSinPosicion = Assert.Single(page.Vm.Mesas, m => m.Numero == 3);
        Assert.Equal(FakeMapaZonasServicio.TerrazaId, mesaSinPosicion.ZonaId);
        Assert.True(mesaSinPosicion.EsUbicacionSugerida);
        Assert.NotNull(mesaSinPosicion.PosicionX);
        Assert.NotNull(mesaSinPosicion.PosicionY);
        Assert.Equal("Redonda", mesaSinPosicion.Forma);
        Assert.True(page.Vm.PuedeEditar);
    }

    [Fact]
    public async Task OnGetAsync_Cuando_No_Hay_Zonas_Pero_Hay_Mesas_Usa_Salon_Principal_Solo_Lectura()
    {
        var mesasServicio = new FakeMapaMesasServicio
        {
            Mesas =
            [
                new() { Id = Guid.NewGuid(), Numero = 10, Capacidad = 4, Estado = "Disponible", Activa = true }
            ]
        };
        var zonasServicio = new FakeMapaZonasServicio { Zonas = [] };

        var page = new MapaModel(mesasServicio, zonasServicio, NullLogger<MapaModel>.Instance);
        SetUserRoles(page, "Encargado");

        await page.OnGetAsync();

        var zona = Assert.Single(page.Vm.Zonas);
        Assert.Equal("Salón principal", zona.Nombre);
        Assert.True(page.Vm.UsaZonaSugerida);
        Assert.False(page.Vm.PuedeEditar);
        var mesa = Assert.Single(page.Vm.Mesas);
        Assert.Equal(zona.Id, mesa.ZonaId);
        Assert.True(mesa.EsUbicacionSugerida);
    }

    [Fact]
    public void MesaMapaItemVm_Inactiva_Usa_Estado_Visual_Inactivo()
    {
        var mesa = new MesaMapaItemVm
        {
            Numero = 8,
            Capacidad = 2,
            Estado = "Disponible",
            Activa = false
        };

        Assert.Equal("Inactiva", mesa.EstadoVisual);
        Assert.Equal("lmd-mapa--inactiva", mesa.ClaseUrgencia);
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
            ZonaId = FakeMapaZonasServicio.TerrazaId,
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
            ZonaId = FakeMapaZonasServicio.TerrazaId,
            Forma = "Redonda"
        };

        var result = await page.OnPostActualizarPosicionAsync(request);

        var json = Assert.IsType<JsonResult>(result);
        Assert.Equal(403, json.StatusCode);
    }

    [Fact]
    public async Task OnPostCambiarEstado_Mesero_NoAutorizado_Debe_Retornar_403()
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
        Assert.Equal(403, json.StatusCode);
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
    public List<MesaDto> Mesas { get; set; } =
    [
        new() { Id = Guid.NewGuid(), Numero = 1, Capacidad = 4, Estado = "Disponible", Activa = true, PosicionX = 10, PosicionY = 20, ZonaId = FakeMapaZonasServicio.TerrazaId, Forma = "Redonda", Rotacion = 0 },
        new() { Id = Guid.NewGuid(), Numero = 2, Capacidad = 6, Estado = "Ocupada", Activa = true, PosicionX = 50, PosicionY = 50, ZonaId = FakeMapaZonasServicio.InteriorId, Forma = "Cuadrada", Rotacion = 45 },
        new() { Id = Guid.NewGuid(), Numero = 3, Capacidad = 2, Estado = "Disponible", Activa = true }
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
    public static readonly Guid TerrazaId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid InteriorId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    public List<ZonaSalonDto> Zonas { get; set; } =
    [
        new() { Id = TerrazaId, Nombre = "Terraza", Orden = 1, Activa = true },
        new() { Id = InteriorId, Nombre = "Interior", Orden = 2, Activa = true }
    ];

    public Task<List<ZonaSalonDto>> ListarActivasAsync(CancellationToken cancelacion = default)
        => Task.FromResult(Zonas);

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
