using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Web.Hubs;
using LaMesaDelDuque.Web.Models.Operaciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace LaMesaDelDuque.Web.Pages.Operaciones.Pedidos;

[Authorize(Roles = "Administrador,Encargado,Cajero")]
public class IndexModel : PageModel
{
    private readonly IPedidosServicio _pedidosServicio;
    private readonly ICatalogoProductosServicio _catalogoProductosServicio;
    private readonly IMesasServicio _mesasServicio;
    private readonly IRecetasProductosServicio _recetasServicio;
    private readonly ITicketServicio _ticketServicio;
    private readonly IAlergenoServicio _alergenoServicio;
    private readonly IHubContext<PedidosHub> _hubContext;

    public IndexModel(
        IPedidosServicio pedidosServicio,
        ICatalogoProductosServicio catalogoProductosServicio,
        IMesasServicio mesasServicio,
        IRecetasProductosServicio recetasServicio,
        ITicketServicio ticketServicio,
        IAlergenoServicio alergenoServicio,
        IHubContext<PedidosHub> hubContext)
    {
        _pedidosServicio = pedidosServicio;
        _catalogoProductosServicio = catalogoProductosServicio;
        _mesasServicio = mesasServicio;
        _recetasServicio = recetasServicio;
        _ticketServicio = ticketServicio;
        _alergenoServicio = alergenoServicio;
        _hubContext = hubContext;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? PedidoActualId { get; set; }

    [BindProperty]
    public PedidosPageVm Vm { get; set; } = new();

    [TempData]
    public string? ToastSuccess { get; set; }

    [TempData]
    public string? ToastError { get; set; }

    public async Task OnGetAsync()
    {
        SetUiContext();
        await CargarDatosAsync();
    }

    // ── Crear pedido ──────────────────────────────────────────
    public async Task<IActionResult> OnPostCrearAsync()
    {
        SetUiContext();
        if (Vm.CrearPedido.Lineas.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Debe seleccionar al menos un producto.");
            await CargarDatosAsync();
            return Page();
        }

        if (!Enum.TryParse<TipoServicio>(Vm.CrearPedido.TipoServicio, true, out var tipoServicio))
        {
            ModelState.AddModelError(string.Empty, "Tipo de servicio inválido.");
            await CargarDatosAsync();
            return Page();
        }

        var productosActivos = (await _catalogoProductosServicio.ListarProductosAsync())
            .Where(p => p.Activo)
            .ToDictionary(p => p.Id, p => p);

        var detalles = new List<DetalleCreacionDto>();
        foreach (var linea in Vm.CrearPedido.Lineas)
        {
            if (!productosActivos.TryGetValue(linea.ProductoId, out var producto))
            {
                ModelState.AddModelError(string.Empty, "Debe seleccionar un producto válido para crear el pedido.");
                await CargarDatosAsync();
                return Page();
            }

            detalles.Add(new DetalleCreacionDto
            {
                ProductoId = linea.ProductoId,
                Cantidad = linea.Cantidad,
                PrecioUnitario = producto.Precio
            });
        }

        try
        {
            var mesaId = tipoServicio == TipoServicio.ComerAqui ? Vm.CrearPedido.MesaId : null;
            var pedido = await _pedidosServicio.CrearPedidoAsync(tipoServicio, mesaId, detalles);
            ToastSuccess = "Pedido creado correctamente.";
            return RedirectToPage(new { PedidoActualId = pedido.Id });
        }
        catch (ReglaDominioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ToastError = ex.Message;
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ToastError = ex.Message;
        }

        await CargarDatosAsync();
        return Page();
    }

    // ── Agregar / actualizar / eliminar línea ─────────────────
    public async Task<IActionResult> OnPostAgregarLineaAsync(Guid pedidoId, Guid productoId, int cantidad, decimal precioUnitario)
        => await EjecutarAccionPedidoAsync(async () =>
        {
            var producto = (await _catalogoProductosServicio.ListarProductosAsync())
                .FirstOrDefault(p => p.Id == productoId && p.Activo)
                ?? throw new ArgumentException("Debe seleccionar un producto activo válido.", nameof(productoId));

            await _pedidosServicio.AgregarDetalleAsync(pedidoId, productoId, cantidad, producto.Precio);
            ToastSuccess = "Línea agregada.";
            return RedirectToPage(new { PedidoActualId = pedidoId });
        });

    public async Task<IActionResult> OnPostActualizarCantidadAsync(Guid pedidoId, Guid detalleId, int cantidad)
        => await EjecutarAccionPedidoAsync(async () =>
        {
            await _pedidosServicio.ActualizarCantidadDetalleAsync(pedidoId, detalleId, cantidad);
            ToastSuccess = "Cantidad actualizada.";
            return RedirectToPage(new { PedidoActualId = pedidoId });
        });

    public async Task<IActionResult> OnPostEliminarLineaAsync(Guid pedidoId, Guid detalleId)
        => await EjecutarAccionPedidoAsync(async () =>
        {
            await _pedidosServicio.EliminarDetalleAsync(pedidoId, detalleId);
            ToastSuccess = "Línea eliminada.";
            return RedirectToPage(new { PedidoActualId = pedidoId });
        });

    // ── Cambiar tipo de servicio mid-orden ────────────────────
    public async Task<IActionResult> OnPostCambiarTipoAsync(Guid pedidoId, string tipoServicio)
    {
        SetUiContext();
        try
        {
            if (!Enum.TryParse<TipoServicio>(tipoServicio, true, out var tipo))
            {
                ToastError = "Tipo de servicio inválido.";
                return RedirectToPage(new { PedidoActualId = pedidoId });
            }

            // Solo cambiamos el tipo en el pedido actual; el cambio real
            // (asignar/quitar mesa) lo hace CrearPedidoAsync o la UI lo ajusta.
            // Aquí simplemente reflejamos la intención.
            ToastSuccess = tipo == TipoServicio.ComerAqui
                ? "Modo cambiado a Comer aquí. Asigne una mesa si lo desea."
                : "Modo cambiado a Para llevar.";
            return RedirectToPage(new { PedidoActualId = pedidoId });
        }
        catch (Exception ex)
        {
            ToastError = ex.Message;
            return RedirectToPage(new { PedidoActualId = pedidoId });
        }
    }

    // ── Pago ──────────────────────────────────────────────────
    public async Task<IActionResult> OnPostAbrirPagoAsync(Guid pedidoId)
    {
        SetUiContext();
        await CargarDatosAsync();
        Vm.MostrarPago = true;
        Vm.Pago = new PagoFormVm();
        return Page();
    }

    public async Task<IActionResult> OnPostPagarEfectivoAsync(Guid pedidoId, decimal efectivoRecibido)
    {
        SetUiContext();

        var pedidosActivos = await _pedidosServicio.ListarPedidosActivosAsync();
        var pedido = pedidosActivos.FirstOrDefault(p => p.Id == pedidoId);

        if (pedido is null)
        {
            ToastError = "El pedido ya no está activo.";
            return RedirectToPage();
        }

        if (efectivoRecibido < pedido.Total)
        {
            Vm.MostrarPago = true;
            Vm.Pago.EfectivoRecibido = efectivoRecibido;
            Vm.Pago.Cambio = null;
            ModelState.AddModelError(string.Empty, $"El monto recibido (${efectivoRecibido:F2}) es menor al total (${pedido.Total:F2}).");
            await CargarDatosAsync();
            return Page();
        }

        try
        {
            await _pedidosServicio.PagarPedidoAsync(pedidoId);
            var cambio = efectivoRecibido - pedido.Total;
            ToastSuccess = cambio > 0
                ? $"Pedido pagado. Cambio: ${cambio:F2}"
                : "Pedido pagado correctamente.";
            return RedirectToPage();
        }
        catch (ReglaDominioException ex) { ToastError = ex.Message; }
        catch (ArgumentException ex) { ToastError = ex.Message; }

        return RedirectToPage(new { PedidoActualId = pedidoId });
    }

    // ── Pagar sin efectivo (compatibilidad) ───────────────────
    public async Task<IActionResult> OnPostPagarAsync(Guid pedidoId)
        => await EjecutarAccionPedidoAsync(async () =>
        {
            await _pedidosServicio.PagarPedidoAsync(pedidoId);
            ToastSuccess = "Pedido pagado correctamente.";
            return RedirectToPage();
        });

    public async Task<IActionResult> OnPostCancelarAsync(Guid pedidoId)
        => await EjecutarAccionPedidoAsync(async () =>
        {
            await _pedidosServicio.CancelarPedidoAsync(pedidoId);
            ToastSuccess = "Pedido cancelado.";
            return RedirectToPage();
        });

    public async Task<IActionResult> OnPostMarcarEnPreparacionAsync(Guid pedidoId)
        => await EjecutarAccionPedidoAsync(async () =>
        {
            await _pedidosServicio.MarcarEnPreparacionAsync(pedidoId);
            ToastSuccess = "Pedido marcado en preparación.";
            return RedirectToPage(new { PedidoActualId = pedidoId });
        });

    // ── JSON handlers para SPA (AJAX, sin recarga) ───────────
    public async Task<IActionResult> OnPostCrearJsonAsync()
    {
        if (Vm.CrearPedido.Lineas.Count == 0 || Vm.CrearPedido.Lineas[0].ProductoId == Guid.Empty)
            return BadRequest("Debe seleccionar un producto.");

        Enum.TryParse<TipoServicio>(Vm.CrearPedido.TipoServicio, true, out var tipoServicio);
        var mesaId = tipoServicio == TipoServicio.ComerAqui ? Vm.CrearPedido.MesaId : null;

        var prods = (await _catalogoProductosServicio.ListarProductosAsync()).Where(p => p.Activo).ToDictionary(p => p.Id);
        var detalles = new List<DetalleCreacionDto>();

        foreach (var l in Vm.CrearPedido.Lineas)
        {
            if (!prods.TryGetValue(l.ProductoId, out var prod)) return BadRequest("Producto inválido.");
            detalles.Add(new DetalleCreacionDto { ProductoId = l.ProductoId, Cantidad = l.Cantidad, PrecioUnitario = prod.Precio, Notas = l.Notas, ModificacionesJson = l.ModificacionesJson });
        }

        try
        {
            var pedido = await _pedidosServicio.CrearPedidoAsync(tipoServicio, mesaId, detalles);
            var detallesResponse = pedido.Detalles.Select(d => new
            {
                id = d.Id, productoId = d.ProductoId,
                productoNombre = d.ProductoNombre, cantidad = d.Cantidad, precioUnitario = d.PrecioUnitario
            });
            return new JsonResult(new { pedidoId = pedido.Id, total = pedido.Total, detalles = detallesResponse });
        }
        catch (Exception ex) { return BadRequest(ErrorSeguro(ex)); }
    }

    public async Task<IActionResult> OnPostEnviarMasJsonAsync()
    {
        if (!Guid.TryParse(Request.Form["pedidoId"].FirstOrDefault(), out var pedidoId))
            return BadRequest(new { error = "pedidoId inválido." });

        if (Vm.CrearPedido.Lineas.Count == 0 || Vm.CrearPedido.Lineas[0].ProductoId == Guid.Empty)
            return BadRequest(new { error = "Debe incluir al menos un ítem." });

        var prods = (await _catalogoProductosServicio.ListarProductosAsync()).Where(p => p.Activo).ToDictionary(p => p.Id);
        var items = new List<DetalleCreacionDto>();

        foreach (var l in Vm.CrearPedido.Lineas)
        {
            if (!prods.TryGetValue(l.ProductoId, out var prod)) return BadRequest(new { error = "Producto inválido." });
            items.Add(new DetalleCreacionDto { ProductoId = l.ProductoId, Cantidad = l.Cantidad, PrecioUnitario = prod.Precio, Notas = l.Notas, ModificacionesJson = l.ModificacionesJson });
        }

        try
        {
            await _pedidosServicio.AgregarItemsAsync(pedidoId, items);
            var pedido = await _pedidosServicio.ObtenerPedidoAsync(pedidoId);
            var detallesResponse = pedido?.Detalles.Select(d => new
            {
                id = d.Id, productoId = d.ProductoId,
                productoNombre = d.ProductoNombre, cantidad = d.Cantidad, precioUnitario = d.PrecioUnitario
            });
            return new JsonResult(new { ok = true, total = pedido?.Total ?? 0, detalles = detallesResponse });
        }
        catch (Exception ex) { return BadRequest(ErrorSeguro(ex)); }
    }

    public async Task<IActionResult> OnPostAgregarLineaJsonAsync(Guid pedidoId, Guid productoId, int cantidad, string? notas = null, string? modificacionesJson = null)
    {
        try
        {
            var prods = await _catalogoProductosServicio.ListarProductosAsync();
            var prod = prods.FirstOrDefault(p => p.Id == productoId && p.Activo)
                ?? throw new ArgumentException("Producto no encontrado.");
            await _pedidosServicio.AgregarDetalleAsync(pedidoId, productoId, cantidad, prod.Precio, notas, modificacionesJson);
            return new JsonResult(new { ok = true });
        }
        catch (Exception ex) { return BadRequest(ErrorSeguro(ex)); }
    }

    public async Task<IActionResult> OnPostEliminarLineaJsonAsync(Guid pedidoId, Guid detalleId)
    {
        try
        {
            await _pedidosServicio.EliminarDetalleAsync(pedidoId, detalleId);
            return new JsonResult(new { ok = true });
        }
        catch (Exception ex) { return BadRequest(ErrorSeguro(ex)); }
    }

    public async Task<IActionResult> OnPostActualizarCantidadJsonAsync(Guid pedidoId, Guid detalleId, int cantidad)
    {
        try
        {
            await _pedidosServicio.ActualizarCantidadDetalleAsync(pedidoId, detalleId, cantidad);
            return new JsonResult(new { ok = true });
        }
        catch (Exception ex) { return BadRequest(ErrorSeguro(ex)); }
    }

    public async Task<IActionResult> OnPostPagarEfectivoJsonAsync(Guid pedidoId, decimal efectivoRecibido)
    {
        try
        {
            var pedidos = await _pedidosServicio.ListarPedidosActivosAsync();
            var pedido = pedidos.FirstOrDefault(p => p.Id == pedidoId)
                ?? throw new ArgumentException("Pedido no encontrado o no está activo.");

            if (pedido.Estado is "Cancelado" or "Pagado" or "Despachado")
                return BadRequest($"El pedido ya fue {pedido.Estado.ToLower()}.");

            if (efectivoRecibido < pedido.Total)
                return BadRequest($"Faltan ${pedido.Total - efectivoRecibido:F2}");

            await _pedidosServicio.PagarPedidoAsync(pedidoId);
            var cambio = efectivoRecibido - pedido.Total;
            return new JsonResult(new { ok = true, mensaje = cambio > 0 ? $"Pedido pagado. Cambio: ${cambio:F2}" : "Pedido pagado correctamente." });
        }
        catch (Exception ex) { return BadRequest(ErrorSeguro(ex)); }
    }

    public async Task<IActionResult> OnPostPagarJsonAsync(Guid pedidoId, string? metodoPago = null, decimal? monto = null, string? referencia = null)
    {
        try
        {
            if (pedidoId == Guid.Empty)
                return BadRequest(ErrorSeguro(new ArgumentException("ID de pedido inválido.")));

            var pedidos = await _pedidosServicio.ListarPedidosActivosAsync();
            var pedido = pedidos.FirstOrDefault(p => p.Id == pedidoId)
                ?? throw new ArgumentException("Pedido no encontrado o no está activo.");

            if (pedido.Estado is "Cancelado" or "Pagado" or "Despachado")
                return BadRequest(ErrorSeguro(new InvalidOperationException($"El pedido ya fue {pedido.Estado.ToLower()}.")));

            var metodoStr = metodoPago?.ToLower() ?? "efectivo";
            var metodoEnum = metodoStr switch
            {
                "tarjeta" => MetodoPago.Tarjeta,
                "qr" or "transferencia" => MetodoPago.Transferencia,
                _ => MetodoPago.Efectivo
            };

            if (metodoEnum == MetodoPago.Efectivo && monto.HasValue && monto.Value < pedido.Total)
                return BadRequest(ErrorSeguro(new ArgumentException($"Faltan ${pedido.Total - monto.Value:F2}")));

            await _pedidosServicio.PagarPedidoAsync(pedidoId, metodoEnum, referencia);

            var cambio = monto.HasValue && metodoEnum == MetodoPago.Efectivo ? monto.Value - pedido.Total : 0;
            var mensaje = cambio > 0 ? $"Pedido pagado. Cambio: ${cambio:F2}" : "Pedido pagado correctamente.";

            string? ticketHtml = null;
            try { ticketHtml = await _ticketServicio.GenerarHtmlTicketAsync(pedidoId); }
            catch { /* no bloquear el pago si el ticket falla */ }

            return new JsonResult(new { ok = true, mensaje, cambio, ticketHtml });
        }
        catch (Exception ex) { return BadRequest(ErrorSeguro(ex)); }
    }

    public async Task<IActionResult> OnPostCancelarJsonAsync(Guid pedidoId)
    {
        try
        {
            if (pedidoId == Guid.Empty)
                return BadRequest(ErrorSeguro(new ArgumentException("ID de pedido inválido.")));
            await _pedidosServicio.CancelarPedidoAsync(pedidoId);
            return new JsonResult(new { ok = true, mensaje = "Pedido cancelado." });
        }
        catch (Exception ex) { return BadRequest(ErrorSeguro(ex)); }
    }

    public async Task<IActionResult> OnPostAnularPagoJsonAsync(Guid pedidoId)
    {
        try
        {
            if (pedidoId == Guid.Empty)
                return BadRequest(ErrorSeguro(new ArgumentException("ID de pedido inválido.")));
            await _pedidosServicio.AnularPagoAsync(pedidoId);
            return new JsonResult(new { ok = true, mensaje = "Pago anulado." });
        }
        catch (Exception ex) { return BadRequest(ErrorSeguro(ex)); }
    }

    // ── Cuentas y pago dividido (JSON) ────────────────────────

    public async Task<IActionResult> OnPostMarcarEnCobroJsonAsync(Guid pedidoId)
    {
        try
        {
            await _pedidosServicio.MarcarEnCobroAsync(pedidoId);
            await _hubContext.Clients.Group($"pedido-{pedidoId}").SendAsync("EstadoCambiado", pedidoId, "EnCobro");
            return new JsonResult(new { ok = true });
        }
        catch (Exception ex) { return BadRequest(ErrorSeguro(ex)); }
    }

    public async Task<IActionResult> OnPostCrearCuentasJsonAsync(Guid pedidoId, int cantidad)
    {
        try
        {
            var cuentas = await _pedidosServicio.CrearCuentasAsync(pedidoId, cantidad);
            await _hubContext.Clients.Group($"pedido-{pedidoId}").SendAsync("CuentasCreadas", pedidoId, cuentas);
            return new JsonResult(cuentas);
        }
        catch (Exception ex) { return BadRequest(ErrorSeguro(ex)); }
    }

    public async Task<IActionResult> OnPostCrearCuentasConItemsJsonAsync([FromBody] CrearCuentasConItemsRequest request)
    {
        try
        {
            if (request?.Asignaciones == null || request.Asignaciones.Count < 2)
                return BadRequest("Se requieren al menos 2 cuentas.");

            if (request.PedidoId == Guid.Empty)
                return BadRequest("El ID del pedido es requerido.");

            // Validar que el pedido existe y está en estado pagable
            var pedidos = await _pedidosServicio.ListarPedidosActivosAsync();
            var pedido = pedidos.FirstOrDefault(p => p.Id == request.PedidoId)
                ?? throw new ArgumentException("Pedido no encontrado o no está activo.");
            if (pedido.Estado is "Cancelado" or "Pagado" or "Despachado")
                return BadRequest($"El pedido ya fue {pedido.Estado.ToLower()}.");

            var asignaciones = request.Asignaciones.ToDictionary(
                a => a.CuentaNumero,
                a => a.Items.Select(i => (i.DetalleId, i.Cantidad)).ToList()
            );

            var cuentas = await _pedidosServicio.CrearCuentasConItemsAsync(request.PedidoId, asignaciones);
            await _hubContext.Clients.Group($"pedido-{request.PedidoId}").SendAsync("CuentasCreadas", request.PedidoId, cuentas);
            return new JsonResult(cuentas);
        }
        catch (Exception ex) { return BadRequest(ErrorSeguro(ex)); }
    }

    public async Task<IActionResult> OnPostObtenerCuentasJsonAsync(Guid pedidoId)
    {
        try
        {
            var cuentas = await _pedidosServicio.ObtenerCuentasAsync(pedidoId);
            return new JsonResult(cuentas);
        }
        catch (Exception ex) { return BadRequest(ErrorSeguro(ex)); }
    }

    public async Task<IActionResult> OnPostPagarCuentaJsonAsync(Guid cuentaId, string metodoPago, decimal propinaMonto)
    {
        try
        {
            if (!Enum.TryParse<MetodoPago>(metodoPago, true, out var metodo))
                return BadRequest("Método de pago inválido.");
            var cuenta = await _pedidosServicio.PagarCuentaAsync(cuentaId, metodo, propinaMonto);
            return new JsonResult(cuenta);
        }
        catch (Exception ex) { return BadRequest(ErrorSeguro(ex)); }
    }

    // ── Ticket PDF / HTML ────────────────────────────────────
    public async Task<IActionResult> OnPostTicketHtmlJsonAsync(Guid pedidoId)
    {
        try
        {
            var html = await _ticketServicio.GenerarHtmlTicketAsync(pedidoId);
            return new JsonResult(new { ok = true, html });
        }
        catch (Exception ex) { return BadRequest(ErrorSeguro(ex)); }
    }

    // ── Mesas (JSON para refresco SPA) ─────────────────────
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
                    PedidoActualId = tab?.Id,
                    PedidoTotal = tab?.Total,
                    PedidoEstado = tab?.Estado,
                    PedidoFechaCreacion = tab?.FechaCreacion,
                    GraciaHasta = m.GraciaHasta
                };
            });
            return new JsonResult(new { mesas = data });
        }
        catch (Exception ex) { return BadRequest(ErrorSeguro(ex)); }
    }

    public async Task<IActionResult> OnGetDetallesPedidoJsonAsync(Guid pedidoId)
    {
        try
        {
            var pedido = await _pedidosServicio.ObtenerPedidoAsync(pedidoId);
            if (pedido is null) return NotFound(new { error = "Pedido no encontrado." });
            var detalles = pedido.Detalles.Select(d => new
            {
                id = d.Id,
                productoId = d.ProductoId,
                productoNombre = d.ProductoNombre,
                cantidad = d.Cantidad,
                precioUnitario = d.PrecioUnitario,
                subtotal = d.Subtotal
            });
            return new JsonResult(new { detalles, total = pedido.Total });
        }
        catch (Exception ex) { return BadRequest(ErrorSeguro(ex)); }
    }

    // ── Alérgenos por producto (JSON) ─────────────────────────
    public async Task<IActionResult> OnGetAlergenosProductoJsonAsync(Guid productoId)
    {
        try
        {
            var alergenos = await _alergenoServicio.ObtenerPorProductoAsync(productoId);
            return new JsonResult(alergenos);
        }
        catch (Exception ex) { return BadRequest(ErrorSeguro(ex)); }
    }

    // ── Ingredientes y modificaciones (JSON) ─────────────────
    public async Task<IActionResult> OnGetIngredientesProductoJsonAsync(Guid productoId)
    {
        try
        {
            var receta = await _recetasServicio.ObtenerPorProductoIdAsync(productoId);
            if (receta is null) return new JsonResult(new { ingredientes = Array.Empty<object>(), instrucciones = "" });
            return new JsonResult(new
            {
                ingredientes = receta.Ingredientes.Select(i => new { id = i.IngredienteId, nombre = i.IngredienteNombre, cantidad = i.CantidadRequerida }),
                receta.Instrucciones
            });
        }
        catch (Exception ex) { return BadRequest(ErrorSeguro(ex)); }
    }

    public async Task<IActionResult> OnPostObtenerIngredientesJsonAsync(Guid productoId)
    {
        try
        {
            var receta = await _recetasServicio.ObtenerPorProductoIdAsync(productoId);
            if (receta is null) return new JsonResult(new { ingredientes = Array.Empty<object>(), instrucciones = "" });
            return new JsonResult(new
            {
                ingredientes = receta.Ingredientes.Select(i => new { i.IngredienteId, i.IngredienteNombre, i.CantidadRequerida }),
                receta.Instrucciones
            });
        }
        catch (Exception ex) { return BadRequest(ErrorSeguro(ex)); }
    }

    // ── Helpers ───────────────────────────────────────────────
    private async Task<IActionResult> EjecutarAccionPedidoAsync(Func<Task<IActionResult>> accion)
    {
        SetUiContext();
        try { return await accion(); }
        catch (ReglaDominioException ex) { ToastError = ex.Message; }
        catch (ArgumentException ex) { ToastError = ex.Message; }
        return RedirectToPage(new { PedidoActualId });
    }

    private async Task CargarDatosAsync()
    {
        try
        {
            var productos = await _catalogoProductosServicio.ListarProductosAsync();
            Vm.ProductosDisponibles = productos.Where(p => p.Activo).OrderBy(p => p.CategoriaNombre).ThenBy(p => p.Nombre).ToList();

            var mesas = await _mesasServicio.ListarMesasAsync();
            Vm.MesasDisponibles = mesas.Where(m => m.Activa).OrderBy(m => m.Numero).ToList();

            Vm.PedidosActivos = await _pedidosServicio.ListarPedidosActivosAsync();
            Vm.PedidoActual = PedidoActualId.HasValue
                ? Vm.PedidosActivos.FirstOrDefault(p => p.Id == PedidoActualId.Value)
                : Vm.PedidosActivos.OrderByDescending(p => p.Total).FirstOrDefault();

            Vm.CrearPedido.TipoServicio = Vm.CrearPedido.TipoServicio is "ParaLlevar" or "ComerAqui" ? Vm.CrearPedido.TipoServicio : "ComerAqui";

            if (Vm.CrearPedido.Lineas.Count == 0)
                Vm.CrearPedido.Lineas.Add(new LineaPedidoFormVm());
        }
        catch (Exception ex)
        {
            Vm.ProductosDisponibles = [];
            Vm.MesasDisponibles = [];
            Vm.PedidosActivos = [];
            ToastError = $"Error al cargar datos: {ex.Message}";
        }
    }

    private void SetUiContext()
    {
        if (ViewData is not null) ViewData["ActiveTab"] = "Pedidos";
    }

    private static object ErrorSeguro(Exception ex)
    {
        var mensaje = ex switch
        {
            ArgumentException => ex.Message,
            InvalidOperationException => ex.Message,
            ReglaDominioException => ex.Message,
            _ => "Ocurrió un error interno al procesar la solicitud."
        };

        return new { error = mensaje };
    }
}
