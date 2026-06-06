using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Web.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace LaMesaDelDuque.Web.Pages.Operaciones.Mesero;

[Authorize(Roles = "Administrador,Encargado,Mesero")]
public class IndexModel : PageModel
{
    private readonly IPedidosServicio _pedidosServicio;
    private readonly ICatalogoProductosServicio _catalogoProductosServicio;
    private readonly IMesasServicio _mesasServicio;
    private readonly IRecetasProductosServicio _recetasServicio;
    private readonly IHubContext<PedidosHub> _hubContext;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        IPedidosServicio pedidosServicio,
        ICatalogoProductosServicio catalogoProductosServicio,
        IMesasServicio mesasServicio,
        IRecetasProductosServicio recetasServicio,
        IHubContext<PedidosHub> hubContext,
        ILogger<IndexModel> logger)
    {
        _pedidosServicio = pedidosServicio;
        _catalogoProductosServicio = catalogoProductosServicio;
        _mesasServicio = mesasServicio;
        _recetasServicio = recetasServicio;
        _hubContext = hubContext;
        _logger = logger;
    }

    public List<ProductoDto> ProductosDisponibles { get; set; } = [];
    public HashSet<Guid> ProductosConReceta { get; private set; } = [];

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Mesero";
        try
        {
            var productos = await _catalogoProductosServicio.ListarProductosAsync();
            ProductosDisponibles = productos.Where(p => p.Activo)
                .OrderBy(p => p.CategoriaNombre).ThenBy(p => p.Nombre).ToList();
            ProductosConReceta = [];
            foreach (var producto in ProductosDisponibles)
            {
                var receta = await _recetasServicio.ObtenerPorProductoIdAsync(producto.Id);
                if (receta is not null && receta.Ingredientes.Count > 0)
                    ProductosConReceta.Add(producto.Id);
            }
        }
        catch { ProductosDisponibles = []; ProductosConReceta = []; }
    }

    // ── Mesas ─────────────────────────────────────────────────────────────────
    public async Task<IActionResult> OnGetMesasJsonAsync()
    {
        try
        {
            var mesas = await _mesasServicio.ListarMesasAsync();
            var pedidos = await _pedidosServicio.ListarPedidosActivosAsync();
            var tabPorMesa = pedidos
                .Where(p => p.MesaId.HasValue)
                .GroupBy(p => p.MesaId!.Value)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.FechaCreacion).First());

            var data = mesas.Where(m => m.Activa).Select(m =>
            {
                tabPorMesa.TryGetValue(m.Id, out var tab);
                return new
                {
                    m.Id, m.Numero, m.Capacidad, m.Estado,
                    Zona = m.Capacidad <= 2 ? "Pequeña" : m.Capacidad <= 4 ? "Mediana" : "Grande",
                    PedidoActualId      = tab?.Id,
                    PedidoTotal         = tab?.Total,
                    PedidoEstado        = tab?.Estado,
                    PedidoFechaCreacion = tab?.FechaCreacion,
                    GraciaHasta         = m.GraciaHasta
                };
            });
            return new JsonResult(new { mesas = data });
        }
        catch (ReglaDominioException ex) { return StatusCode(422, new { ok = false, error = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { ok = false, error = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error en handler JSON"); return StatusCode(500, new { ok = false, error = "Ocurrió un error interno." }); }
    }

    // ── Detalles de pedido ────────────────────────────────────────────────────
    public async Task<IActionResult> OnGetDetallesPedidoJsonAsync(Guid pedidoId)
    {
        try
        {
            var pedido = await _pedidosServicio.ObtenerPedidoAsync(pedidoId);
            if (pedido is null) return NotFound(new { error = "Pedido no encontrado." });
            var detalles = pedido.Detalles.Select(d => new
            {
                id             = d.Id,
                productoId     = d.ProductoId,
                productoNombre = d.ProductoNombre,
                cantidad       = d.Cantidad,
                precioUnitario = d.PrecioUnitario,
                subtotal       = d.Subtotal
            });
            return new JsonResult(new { detalles, total = pedido.Total, estado = pedido.Estado });
        }
        catch (ReglaDominioException ex) { return StatusCode(422, new { ok = false, error = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { ok = false, error = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error en handler JSON"); return StatusCode(500, new { ok = false, error = "Ocurrió un error interno." }); }
    }

    // ── Agregar ítem ──────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostAgregarLineaJsonAsync(
        Guid pedidoId, Guid productoId, int cantidad,
        string? notas = null, string? modificacionesJson = null)
    {
        try
        {
            var prods = await _catalogoProductosServicio.ListarProductosAsync();
            var prod = prods.FirstOrDefault(p => p.Id == productoId && p.Activo)
                ?? throw new ArgumentException("Producto no encontrado.");
            await _pedidosServicio.AgregarDetalleAsync(pedidoId, productoId, cantidad, prod.Precio, notas, modificacionesJson);
            var pedido = await _pedidosServicio.ObtenerPedidoAsync(pedidoId);
            return new JsonResult(new { ok = true, total = pedido?.Total ?? 0 });
        }
        catch (ReglaDominioException ex) { return StatusCode(422, new { ok = false, error = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { ok = false, error = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error en handler JSON"); return StatusCode(500, new { ok = false, error = "Ocurrió un error interno." }); }
    }

    // ── Actualizar cantidad de ítem ───────────────────────────────────────────
    public async Task<IActionResult> OnPostActualizarCantidadJsonAsync(
        Guid pedidoId, Guid detalleId, int cantidad)
    {
        try
        {
            PedidoDto? pedido;
            if (cantidad <= 0)
            {
                pedido = await _pedidosServicio.EliminarDetalleAsync(pedidoId, detalleId);
            }
            else
            {
                await _pedidosServicio.ActualizarCantidadDetalleAsync(pedidoId, detalleId, cantidad);
                pedido = await _pedidosServicio.ObtenerPedidoAsync(pedidoId);
            }
            var detalles = pedido?.Detalles.Select(d => new
            {
                id             = d.Id,
                productoId     = d.ProductoId,
                productoNombre = d.ProductoNombre,
                cantidad       = d.Cantidad,
                precioUnitario = d.PrecioUnitario,
                subtotal       = d.Subtotal
            });
            return new JsonResult(new { ok = true, total = pedido?.Total ?? 0, detalles });
        }
        catch (ReglaDominioException ex) { return StatusCode(422, new { ok = false, error = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { ok = false, error = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error en handler JSON"); return StatusCode(500, new { ok = false, error = "Ocurrió un error interno." }); }
    }

    // ── Anular ítem ───────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostEliminarLineaJsonAsync(Guid pedidoId, Guid detalleId)
    {
        try
        {
            await _pedidosServicio.EliminarDetalleAsync(pedidoId, detalleId);
            var pedido = await _pedidosServicio.ObtenerPedidoAsync(pedidoId);
            return new JsonResult(new { ok = true, total = pedido?.Total ?? 0 });
        }
        catch (ReglaDominioException ex) { return StatusCode(422, new { ok = false, error = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { ok = false, error = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error en handler JSON"); return StatusCode(500, new { ok = false, error = "Ocurrió un error interno." }); }
    }

    // ── Solicitar cuenta ──────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostMarcarEnCobroJsonAsync(Guid pedidoId)
    {
        try
        {
            await _pedidosServicio.MarcarEnCobroAsync(pedidoId);
            await _hubContext.Clients.Group($"pedido-{pedidoId}").SendAsync("EstadoCambiado", pedidoId, "EnCobro");
            return new JsonResult(new { ok = true });
        }
        catch (ReglaDominioException ex) { return StatusCode(422, new { ok = false, error = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { ok = false, error = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error en handler JSON"); return StatusCode(500, new { ok = false, error = "Ocurrió un error interno." }); }
    }

    // ── Sentar mesa — crear pedido con primer ítem ───────────────────────────
    public async Task<IActionResult> OnPostCrearConItemsJsonAsync(Guid mesaId, string? itemsJson)
    {
        try
        {
            if (mesaId == Guid.Empty)
                return BadRequest(ErrorSeguro(new ArgumentException("Mesa inválida.")));

            List<ItemCarritoDto> items;
            try
            {
                items = string.IsNullOrWhiteSpace(itemsJson)
                    ? []
                    : JsonSerializer.Deserialize<List<ItemCarritoDto>>(itemsJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
            }
            catch { return BadRequest(ErrorSeguro(new ArgumentException("Formato de ítems inválido."))); }

            if (items.Count == 0)
                return BadRequest(ErrorSeguro(new ArgumentException("Debe incluir al menos un ítem.")));

            var prods = await _catalogoProductosServicio.ListarProductosAsync();
            var prodsDict = prods.Where(p => p.Activo).ToDictionary(p => p.Id);

            var detalles = new List<DetalleCreacionDto>();
            foreach (var item in items)
            {
                if (!prodsDict.TryGetValue(item.ProductoId, out var prod))
                    return BadRequest(ErrorSeguro(new ArgumentException("Producto no encontrado.")));
                detalles.Add(new DetalleCreacionDto
                {
                    ProductoId     = item.ProductoId,
                    Cantidad       = item.Cantidad,
                    PrecioUnitario = prod.Precio,
                    Notas = item.Notas,
                    ModificacionesJson = item.ModificacionesJson
                });
            }

            var pedido = await _pedidosServicio.CrearPedidoAsync(TipoServicio.ComerAqui, mesaId, detalles);
            return new JsonResult(new { ok = true, pedidoId = pedido.Id, total = pedido.Total });
        }
        catch (ReglaDominioException ex) { return StatusCode(422, new { ok = false, error = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { ok = false, error = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error en handler JSON"); return StatusCode(500, new { ok = false, error = "Ocurrió un error interno." }); }
    }

    // ── Cobrar en mesa ────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostPagarJsonAsync(
        Guid pedidoId, string? metodoPago = null, decimal? monto = null, string? referencia = null)
    {
        try
        {
            if (pedidoId == Guid.Empty)
                return BadRequest(ErrorSeguro(new ArgumentException("ID de pedido inválido.")));

            var pedido = await _pedidosServicio.ObtenerPedidoAsync(pedidoId)
                ?? throw new ArgumentException("Pedido no encontrado.");

            if (pedido.Estado == "Pagado")
                return new JsonResult(new { ok = true, mensaje = "Pedido ya estaba pagado.", yaPagado = true });

            if (pedido.Estado is "Cancelado" or "Despachado")
                return BadRequest(ErrorSeguro(new InvalidOperationException($"El pedido ya fue {pedido.Estado.ToLower()}.")));

            var metodo = metodoPago?.ToLower() ?? "efectivo";
            var metodoEnum = metodo switch
            {
                "tarjeta" => MetodoPago.Tarjeta,
                "qr" or "transferencia" => MetodoPago.Transferencia,
                _ => MetodoPago.Efectivo
            };

            if (metodoEnum == MetodoPago.Efectivo && monto.HasValue && monto.Value < pedido.Total)
                return BadRequest(ErrorSeguro(new ArgumentException($"Faltan ${pedido.Total - monto.Value:F2}")));

            await _pedidosServicio.PagarPedidoAsync(pedidoId, metodoEnum, referencia);

            var cambio = monto.HasValue && metodoEnum == MetodoPago.Efectivo ? monto.Value - pedido.Total : 0;
            return new JsonResult(new
            {
                ok = true,
                mensaje = cambio > 0 ? $"Pedido pagado. Cambio: ${cambio:F2}" : "Pedido pagado correctamente."
            });
        }
        catch (ReglaDominioException ex) { return StatusCode(422, new { ok = false, error = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { ok = false, error = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error en handler JSON"); return StatusCode(500, new { ok = false, error = "Ocurrió un error interno." }); }
    }


    public async Task<IActionResult> OnPostCambiarEstadoMesaJsonAsync(Guid mesaId, string estado = "Disponible")
    {
        try
        {
            if (mesaId == Guid.Empty)
                return BadRequest(ErrorSeguro(new ArgumentException("Mesa inválida.")));

            await _mesasServicio.CambiarEstadoMesaAsync(mesaId, estado);
            return new JsonResult(new { ok = true });
        }
        catch (ReglaDominioException ex) { return StatusCode(422, new { ok = false, error = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { ok = false, error = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error en handler JSON"); return StatusCode(500, new { ok = false, error = "Ocurrió un error interno." }); }
    }

    private sealed record ItemCarritoDto(Guid ProductoId, int Cantidad, string? Notas, string? ModificacionesJson);

    // ── Helper ────────────────────────────────────────────────────────────────
    private static object ErrorSeguro(Exception ex)
    {
        var mensaje = ex switch
        {
            ArgumentException        => ex.Message,
            InvalidOperationException => ex.Message,
            ReglaDominioException    => ex.Message,
            _                        => "Ocurrió un error interno al procesar la solicitud."
        };
        return new { error = mensaje };
    }
}
