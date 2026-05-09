using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Web.Models.Operaciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages.Operaciones.Productos;

[Authorize(Roles = "Administrador,Encargado")]
public class IndexModel : PageModel
{
    private readonly ICatalogoProductosServicio _catalogoProductosServicio;

    public IndexModel(ICatalogoProductosServicio catalogoProductosServicio)
    {
        _catalogoProductosServicio = catalogoProductosServicio;
    }

    [BindProperty(SupportsGet = true)]
    public string? Buscar { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? CategoriaId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Estado { get; set; } = "todos";

    [BindProperty]
    public ProductosPageVm Vm { get; set; } = new();

    [TempData]
    public string? ToastSuccess { get; set; }

    [TempData]
    public string? ToastError { get; set; }

    public async Task OnGetAsync()
    {
        SetUiContext();
        await CargarDatosAsync();
    }

    public async Task<IActionResult> OnPostGuardarAsync()
    {
        SetUiContext();
        if (!ModelState.IsValid)
        {
            await CargarDatosAsync();
            return Page();
        }

        try
        {
            if (Vm.Form.Id.HasValue)
            {
                await _catalogoProductosServicio.ActualizarProductoAsync(
                    Vm.Form.Id.Value,
                    Vm.Form.Nombre,
                    Vm.Form.Precio,
                    Vm.Form.CategoriaId,
                    Vm.Form.Descripcion,
                    Vm.Form.ImagenUrl,
                    Vm.Form.TiempoPreparacionMin);
                ToastSuccess = "Producto actualizado correctamente.";
            }
            else
            {
                await _catalogoProductosServicio.CrearProductoAsync(
                    Vm.Form.Nombre,
                    Vm.Form.Precio,
                    Vm.Form.CategoriaId,
                    Vm.Form.Descripcion,
                    Vm.Form.ImagenUrl,
                    Vm.Form.TiempoPreparacionMin ?? 5);
                ToastSuccess = "Producto creado correctamente.";
            }

            return RedirectToPage(new { Buscar, CategoriaId, Estado });
        }
        catch (ReglaDominioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        await CargarDatosAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDesactivarAsync(Guid id)
    {
        SetUiContext();
        try
        {
            await _catalogoProductosServicio.DesactivarProductoAsync(id);
            ToastSuccess = "Producto desactivado correctamente.";
        }
        catch (ReglaDominioException ex)
        {
            ToastError = ex.Message;
        }
        catch (ArgumentException ex)
        {
            ToastError = ex.Message;
        }

        return RedirectToPage(new { Buscar, CategoriaId, Estado });
    }

    private async Task CargarDatosAsync()
    {
        Vm.Categorias = await _catalogoProductosServicio.ListarCategoriasAsync();

        var productos = await _catalogoProductosServicio.ListarProductosAsync();
        if (!string.IsNullOrWhiteSpace(Buscar))
        {
            productos = productos.Where(p => p.Nombre.Contains(Buscar, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (CategoriaId.HasValue)
        {
            productos = productos.Where(p => p.CategoriaId == CategoriaId.Value).ToList();
        }

        if (Estado == "activos")
        {
            productos = productos.Where(p => p.Activo).ToList();
        }
        else if (Estado == "inactivos")
        {
            productos = productos.Where(p => !p.Activo).ToList();
        }

        Vm.Buscar = Buscar;
        Vm.CategoriaId = CategoriaId;
        Vm.Estado = Estado;
        Vm.Productos = productos;
    }

    private void SetUiContext()
    {
        if (ViewData is not null)
        {
            ViewData["ActiveTab"] = "Productos";
        }
    }
}
