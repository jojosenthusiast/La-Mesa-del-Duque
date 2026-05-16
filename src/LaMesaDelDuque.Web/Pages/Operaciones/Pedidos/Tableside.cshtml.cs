using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Web.Models.Operaciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages.Operaciones.Pedidos;

[Authorize(Roles = "Mesero,Encargado,Administrador")]
public class TablesideModel : PageModel
{
    private readonly IPedidosServicio _pedidosServicio;
    private readonly ICatalogoProductosServicio _catalogoProductosServicio;
    private readonly IMesasServicio _mesasServicio;

    public TablesideModel(
        IPedidosServicio pedidosServicio,
        ICatalogoProductosServicio catalogoProductosServicio,
        IMesasServicio mesasServicio)
    {
        _pedidosServicio = pedidosServicio;
        _catalogoProductosServicio = catalogoProductosServicio;
        _mesasServicio = mesasServicio;
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
            return BadRequest("Debe seleccionar un producto.");

        Enum.TryParse<TipoServicio>(Vm.CrearPedido.TipoServicio, true, out var tipoServicio);
        var mesaId = tipoServicio == TipoServicio.ComerAqui ? Vm.CrearPedido.MesaId : null;

        var prods = (await _catalogoProductosServicio.ListarProductosAsync()).Where(p => p.Activo).ToDictionary(p => p.Id);
        var detalles = new List<DetalleCreacionDto>();

        foreach (var l in Vm.CrearPedido.Lineas)
        {
            if (!prods.TryGetValue(l.ProductoId, out var prod)) return BadRequest("Producto inválido.");
            detalles.Add(new DetalleCreacionDto { ProductoId = l.ProductoId, Cantidad = l.Cantidad, PrecioUnitario = prod.Precio });
        }

        try
        {
            var pedido = await _pedidosServicio.CrearPedidoAsync(tipoServicio, mesaId, detalles);
            return new JsonResult(new { pedidoId = pedido.Id, estado = pedido.Estado, lineas = pedido.Detalles });
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    public async Task<IActionResult> OnPostEnviarACocinaJsonAsync(Guid pedidoId)
    {
        try
        {
            await _pedidosServicio.MarcarEnPreparacionAsync(pedidoId);
            return new JsonResult(new { ok = true, estado = "EnPreparacion" });
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
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
        catch (Exception ex) { return BadRequest(ex.Message); }
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
}
