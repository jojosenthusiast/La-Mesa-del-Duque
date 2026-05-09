using LaMesaDelDuque.Aplicacion.Dtos;
using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Web.Models.Operaciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages.Operaciones.Pedidos;

public class IndexModel : PageModel
{
    private readonly IPedidosServicio _pedidosServicio;
    private readonly ICatalogoProductosServicio _catalogoProductosServicio;
    private readonly IMesasServicio _mesasServicio;

    public IndexModel(
        IPedidosServicio pedidosServicio,
        ICatalogoProductosServicio catalogoProductosServicio,
        IMesasServicio mesasServicio)
    {
        _pedidosServicio = pedidosServicio;
        _catalogoProductosServicio = catalogoProductosServicio;
        _mesasServicio = mesasServicio;
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

        var detalles = Vm.CrearPedido.Lineas
            .Select(l => new DetalleCreacionDto { ProductoId = l.ProductoId, Cantidad = l.Cantidad, PrecioUnitario = l.PrecioUnitario })
            .ToList();

        try
        {
            var pedido = await _pedidosServicio.CrearPedidoAsync(tipoServicio, Vm.CrearPedido.MesaId, detalles);
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

    public async Task<IActionResult> OnPostAgregarLineaAsync(Guid pedidoId, Guid productoId, int cantidad, decimal precioUnitario)
        => await EjecutarAccionPedidoAsync(async () =>
        {
            await _pedidosServicio.AgregarDetalleAsync(pedidoId, productoId, cantidad, precioUnitario);
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

    private async Task<IActionResult> EjecutarAccionPedidoAsync(Func<Task<IActionResult>> accion)
    {
        SetUiContext();
        try
        {
            return await accion();
        }
        catch (ReglaDominioException ex)
        {
            ToastError = ex.Message;
        }
        catch (ArgumentException ex)
        {
            ToastError = ex.Message;
        }

        return RedirectToPage(new { PedidoActualId });
    }

    private async Task CargarDatosAsync()
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
    }

    private void SetUiContext()
    {
        if (ViewData is not null)
        {
            ViewData["ActiveTab"] = "Pedidos";
        }
    }
}
