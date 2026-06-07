using System.Text.Json;
using System.Text.Json.Serialization;
using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Notificaciones;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages.Cocina;

[Authorize(Roles = "Cocinero,Encargado,Administrador")]
public class KDSModel : PageModel
{
    private readonly ICocinaServicio _cocinaServicio;
    private readonly ICatalogoProductosServicio _catalogoServicio;
    private readonly INotificadorProductos _notificadorProductos;
    private readonly ILogger<KDSModel> _logger;

    public KDSModel(
        ICocinaServicio cocinaServicio,
        ICatalogoProductosServicio catalogoServicio,
        INotificadorProductos notificadorProductos,
        ILogger<KDSModel> logger)
    {
        _cocinaServicio = cocinaServicio;
        _catalogoServicio = catalogoServicio;
        _notificadorProductos = notificadorProductos;
        _logger = logger;
    }

    public List<OrdenCocinaDto> Ordenes { get; set; } = [];

    public static IReadOnlyList<CookConfig> Cooks { get; } = new List<CookConfig>
    {
        new(1, "Atrasados", "#e74c3c"),
        new(2, "Por vencer", "#f1c40f"),
        new(3, "En tiempo", "#2ecc71")
    };

    public static IReadOnlyDictionary<string, int> StationToColumn { get; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        { "Parrilla", 1 },
        { "Fria", 2 },
        { "Caliente", 3 },
        { "Bar", 2 },
        { "Expo", 1 }
    };

    public record CookConfig(int Id, string Name, string Color);

    public async Task OnGetAsync()
    {
        Ordenes = await _cocinaServicio.ListarPendientesAsync();
    }

    public async Task<IActionResult> OnGetOrdenesJsonAsync(string estacion)
    {
        EstacionCocina? filtro = null;
        if (!string.IsNullOrWhiteSpace(estacion) && estacion != "Todas" && Enum.TryParse<EstacionCocina>(estacion, out var estacionEnum))
        {
            filtro = estacionEnum;
        }

        var ordenes = await _cocinaServicio.ListarPendientesAsync(filtro);
        var opts = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        };
        return new JsonResult(ordenes, opts);
    }

    public async Task<IActionResult> OnGetEstadoActualJsonAsync(string estacion)
    {
        EstacionCocina? filtro = null;
        if (!string.IsNullOrWhiteSpace(estacion) && estacion != "Todas" && Enum.TryParse<EstacionCocina>(estacion, out var estacionEnum))
        {
            filtro = estacionEnum;
        }

        var ordenes = await _cocinaServicio.ListarPendientesAsync(filtro);
        var optsEstado = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        };
        return new JsonResult(new {
            ordenesCocina = ordenes.Select(o => new {
                o.Id,
                o.PedidoId,
                o.ProductoNombre,
                o.Cantidad,
                o.Estado,
                o.HoraRecibido,
                o.Estacion,
                o.Notas,
                o.Alergenos,
                o.IngredientesQuitados,
                o.IngredientesExtra,
                o.MinutosTranscurridos,
                o.MesaNumero,
                o.TipoServicio,
                o.CocineroId,
                o.Curso,
                o.ProductoId,
                o.TiempoPreparacionMin
            }),
            timestamp = DateTime.UtcNow
        }, optsEstado);
    }

    public async Task<IActionResult> OnPostMarcarListoJsonAsync(Guid ordenId)
    {
        try
        {
            var dto = await _cocinaServicio.MarcarListoAsync(ordenId);
            return new JsonResult(new { ok = true });
        }
        catch (ReglaDominioException ex) when (ex.Message.Contains("Ya está listo"))
        {
            return new JsonResult(new { ok = true });
        }
        catch (ReglaDominioException ex)
        {
            return StatusCode(422, new { ok = false, error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { ok = false, error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en OnPostMarcarListoJsonAsync");
            return StatusCode(500, new { ok = false, error = "Ocurrió un error interno." });
        }
    }

    public async Task<IActionResult> OnPostMarcar86JsonAsync([FromBody] Marcar86Request req)
    {
        try
        {
            var productos = await _catalogoServicio.ListarProductosAsync();
            var producto = productos.FirstOrDefault(p => p.Id == req.ProductoId)
                ?? throw new ArgumentException($"No se encontró el producto con ID {req.ProductoId}.");

            await _catalogoServicio.DesactivarProductoAsync(req.ProductoId);
            await _notificadorProductos.NotificarProductoAgotadoAsync(req.ProductoId, producto.Nombre);

            return new JsonResult(new { ok = true, nombre = producto.Nombre });
        }
        catch (ReglaDominioException ex)
        {
            return StatusCode(422, new { ok = false, error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { ok = false, error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en OnPostMarcar86JsonAsync");
            return StatusCode(500, new { ok = false, error = "Ocurrió un error interno." });
        }
    }

    public record Marcar86Request(Guid ProductoId);
}
