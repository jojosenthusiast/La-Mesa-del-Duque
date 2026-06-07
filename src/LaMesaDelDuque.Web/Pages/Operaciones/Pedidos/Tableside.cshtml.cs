using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Web.Models.Operaciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages.Operaciones.Pedidos;

[Authorize(Roles = "Mesero,Cajero,Encargado,Administrador")]
public class TablesideModel : PageModel
{
    private readonly IPedidosServicio _pedidosServicio;
    private readonly ICatalogoProductosServicio _catalogoProductosServicio;
    private readonly IMesasServicio _mesasServicio;
    private readonly ILogger<TablesideModel> _logger;

    public TablesideModel(
        IPedidosServicio pedidosServicio,
        ICatalogoProductosServicio catalogoProductosServicio,
        IMesasServicio mesasServicio,
        ILogger<TablesideModel> logger)
    {
        _pedidosServicio = pedidosServicio;
        _catalogoProductosServicio = catalogoProductosServicio;
        _mesasServicio = mesasServicio;
        _logger = logger;
    }

    [BindProperty]
    public PedidosPageVm Vm { get; set; } = new();

    public async Task OnGetAsync()
    {
        await CargarDatosAsync();
    }

    public async Task<IActionResult> OnPostCrearJsonAsync()
    {
        if (Vm.CrearPedido.Lineas.Count == 0 || Vm.CrearPedido.Lineas[0].ProductoId == Guid.Empty)
            return BadRequest(new { ok = false, error = "Debe seleccionar un producto." });

        if (!Enum.TryParse<TipoServicio>(Vm.CrearPedido.TipoServicio, true, out var tipoServicio))
            tipoServicio = TipoServicio.ComerAqui;
        var mesaId = tipoServicio == TipoServicio.ComerAqui ? Vm.CrearPedido.MesaId : null;

        var prods = (await _catalogoProductosServicio.ListarProductosAsync()).Where(p => p.Activo).ToDictionary(p => p.Id);
        var detalles = new List<DetalleCreacionDto>();

        foreach (var l in Vm.CrearPedido.Lineas)
        {
            if (!prods.TryGetValue(l.ProductoId, out var prod))
                return BadRequest(new { ok = false, error = "Producto inválido." });

            detalles.Add(new DetalleCreacionDto { ProductoId = l.ProductoId, Cantidad = l.Cantidad, PrecioUnitario = prod.Precio });
        }

        try
        {
            var pedido = await _pedidosServicio.CrearPedidoAsync(tipoServicio, mesaId, detalles, datosEntrega: CrearDatosEntregaSiAplica(tipoServicio));
            return new JsonResult(new { pedidoId = pedido.Id, estado = pedido.Estado, lineas = pedido.Detalles });
        }
        catch (ReglaDominioException ex) { return StatusCode(422, new { ok = false, error = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { ok = false, error = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error en handler JSON"); return StatusCode(500, new { ok = false, error = "Ocurrió un error interno." }); }
    }

    public async Task<IActionResult> OnPostEnviarACocinaJsonAsync(Guid pedidoId)
    {
        try
        {
            if (pedidoId == Guid.Empty)
                return BadRequest(new { ok = false, error = "ID de pedido inválido." });

            await _pedidosServicio.MarcarEnPreparacionAsync(pedidoId);
            return new JsonResult(new { ok = true, estado = "EnPreparacion" });
        }
        catch (ReglaDominioException ex) { return StatusCode(422, new { ok = false, error = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { ok = false, error = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error en handler JSON"); return StatusCode(500, new { ok = false, error = "Ocurrió un error interno." }); }
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
        catch (ReglaDominioException ex) { return StatusCode(422, new { ok = false, error = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { ok = false, error = ex.Message }); }
        catch (Exception ex) { _logger.LogError(ex, "Error en handler JSON"); return StatusCode(500, new { ok = false, error = "Ocurrió un error interno." }); }
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

    private async Task CargarDatosAsync()
    {
        var productos = await _catalogoProductosServicio.ListarProductosAsync();
        Vm.ProductosDisponibles = productos.Where(p => p.Activo).OrderBy(p => p.CategoriaNombre).ThenBy(p => p.Nombre).ToList();

        var mesas = await _mesasServicio.ListarMesasAsync();
        Vm.MesasDisponibles = mesas.Where(m => m.Activa).OrderBy(m => m.Numero).ToList();

        Vm.CrearPedido.TipoServicio = "ComerAqui";

        if (Vm.CrearPedido.Lineas.Count == 0)
            Vm.CrearPedido.Lineas.Add(new LineaPedidoFormVm());
    }

    private DatosEntregaDto? CrearDatosEntregaSiAplica(TipoServicio tipoServicio)
    {
        if (tipoServicio != TipoServicio.Delivery)
            return null;

        return new DatosEntregaDto
        {
            ClienteNombre = Vm.CrearPedido.ClienteDeliveryNombre,
            Telefono = Vm.CrearPedido.ClienteDeliveryTelefono,
            Direccion = Vm.CrearPedido.ClienteDeliveryDireccion,
            Referencia = Vm.CrearPedido.ClienteDeliveryReferencia,
            Notas = Vm.CrearPedido.ClienteDeliveryNotas
        };
    }
}
