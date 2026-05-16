using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Web.Pages.Cocina;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;

namespace LaMesaDelDuque.Pruebas.Web;

public class KDSPageTests
{
    [Fact]
    public async Task OnGetAsync_carga_ordenes_pendientes()
    {
        var servicio = new FakeCocinaServicio();
        var page = new KDSModel(servicio);

        await page.OnGetAsync();

        Assert.NotEmpty(page.Ordenes);
        Assert.All(page.Ordenes, o => Assert.NotEqual("Listo", o.Estado));
    }

    [Fact]
    public async Task OnGetOrdenesJsonAsync_sin_estacion_retorna_todas()
    {
        var servicio = new FakeCocinaServicio();
        var page = new KDSModel(servicio);

        var result = await page.OnGetOrdenesJsonAsync("");

        var json = Assert.IsType<JsonResult>(result);
        var ordenesObj = json.Value;
        var ordenes = ordenesObj is List<OrdenCocinaDto> directList ? directList
            : ordenesObj is JsonElement je ? je.Deserialize<List<OrdenCocinaDto>>() : null;
        Assert.NotNull(ordenes);
        Assert.Equal(2, ordenes.Count);
    }

    [Fact]
    public async Task OnGetOrdenesJsonAsync_con_estacion_filtra()
    {
        var servicio = new FakeCocinaServicio();
        var page = new KDSModel(servicio);

        var result = await page.OnGetOrdenesJsonAsync("Parrilla");

        var json = Assert.IsType<JsonResult>(result);
        var ordenesObj = json.Value;
        var ordenes = ordenesObj is List<OrdenCocinaDto> directList ? directList
            : ordenesObj is JsonElement je ? je.Deserialize<List<OrdenCocinaDto>>() : null;
        Assert.NotNull(ordenes);
        Assert.All(ordenes, o => Assert.Equal("Parrilla", o.Estacion));
    }

    [Fact]
    public async Task OnPostMarcarListoJsonAsync_marca_como_listo()
    {
        var servicio = new FakeCocinaServicio();
        var ordenId = servicio.Ordenes[0].Id;
        var page = new KDSModel(servicio);

        var result = await page.OnPostMarcarListoJsonAsync(ordenId);

        var json = Assert.IsType<JsonResult>(result);
        Assert.NotNull(json.Value);
        var orden = servicio.Ordenes.First(o => o.Id == ordenId);
        Assert.Equal("Listo", orden.Estado);
        Assert.Contains(ordenId, servicio.MarcadoComoListo);
    }

    [Fact]
    public async Task OnPostMarcarListoJsonAsync_orden_inexistente_devuelve_bad_request()
    {
        var servicio = new FakeCocinaServicio();
        var page = new KDSModel(servicio);

        var result = await page.OnPostMarcarListoJsonAsync(Guid.NewGuid());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    internal sealed class FakeCocinaServicio : ICocinaServicio
    {
        public List<OrdenCocinaDto> Ordenes { get; } = new()
        {
            new() { Id = Guid.NewGuid(), PedidoId = Guid.NewGuid(), ProductoNombre = "Solomillo", Cantidad = 1, Estacion = "Parrilla", Estado = "Pendiente", HoraRecibido = DateTime.UtcNow.AddMinutes(-3), MesaNumero = 1, TipoServicio = "ComerAqui" },
            new() { Id = Guid.NewGuid(), PedidoId = Guid.NewGuid(), ProductoNombre = "Bruschetta", Cantidad = 2, Estacion = "Fria", Estado = "EnPreparacion", HoraRecibido = DateTime.UtcNow.AddMinutes(-8), MesaNumero = 2, TipoServicio = "ComerAqui" }
        };

        public List<Guid> MarcadoComoListo { get; } = new();
        public List<Guid> Recuperado { get; } = new();
        public List<Guid> Generados { get; } = new();

        public Task GenerarOrdenesAsync(Guid pedidoId, CancellationToken ct = default)
        {
            Generados.Add(pedidoId);
            return Task.CompletedTask;
        }

        public Task<List<OrdenCocinaDto>> ListarPendientesAsync(EstacionCocina? estacion = null, CancellationToken ct = default)
        {
            var query = Ordenes.Where(o => o.Estado != "Listo").AsEnumerable();
            if (estacion.HasValue)
                query = query.Where(o => o.Estacion == estacion.Value.ToString());
            return Task.FromResult(query.ToList());
        }

        public Task<List<OrdenCocinaDto>> ListarListosAsync(CancellationToken ct = default)
            => Task.FromResult(Ordenes.Where(o => o.Estado == "Listo").ToList());

        public Task<OrdenCocinaDto> MarcarListoAsync(Guid ordenId, CancellationToken ct = default)
        {
            var orden = Ordenes.FirstOrDefault(o => o.Id == ordenId)
                ?? throw new ArgumentException($"No se encontró la orden de cocina con ID {ordenId}.", nameof(ordenId));
            orden.Estado = "Listo";
            MarcadoComoListo.Add(ordenId);
            return Task.FromResult(orden);
        }

        public Task<OrdenCocinaDto> RecuperarAsync(Guid ordenId, CancellationToken ct = default)
        {
            var orden = Ordenes.FirstOrDefault(o => o.Id == ordenId)
                ?? throw new ArgumentException($"No se encontró la orden de cocina con ID {ordenId}.", nameof(ordenId));
            orden.Estado = "EnPreparacion";
            Recuperado.Add(ordenId);
            return Task.FromResult(orden);
        }
    }
}
