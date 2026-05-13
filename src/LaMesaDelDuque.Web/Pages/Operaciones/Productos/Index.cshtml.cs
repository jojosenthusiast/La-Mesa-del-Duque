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
    private readonly IWebHostEnvironment _env;

    private static readonly string[] ExtensionesPermitidas = [".jpg", ".jpeg", ".png", ".webp"];
    private const long TamanioMaximoBytes = 5 * 1024 * 1024;

    public IndexModel(ICatalogoProductosServicio catalogoProductosServicio, IWebHostEnvironment env)
    {
        _catalogoProductosServicio = catalogoProductosServicio;
        _env = env;
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

        var archivo = Vm.Form.ImagenFile;
        if (archivo is not null && archivo.Length > 0)
        {
            if (archivo.Length > TamanioMaximoBytes)
            {
                ModelState.AddModelError("Vm.Form.ImagenFile", "La imagen no puede exceder 5MB.");
            }

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!ExtensionesPermitidas.Contains(extension))
            {
                ModelState.AddModelError("Vm.Form.ImagenFile", "Solo se permiten imagenes JPG, PNG o WebP.");
            }
        }

        if (!ModelState.IsValid)
        {
            await CargarDatosAsync();
            return Page();
        }

        try
        {
            string? imagenUrl = Vm.Form.ImagenUrl;
            Guid productoId;

            if (Vm.Form.Id.HasValue)
            {
                productoId = Vm.Form.Id.Value;
                if (Vm.Form.EliminarImagen)
                {
                    EliminarArchivoImagen(productoId);
                    imagenUrl = null;
                }
                else if (archivo is not null && archivo.Length > 0)
                {
                    imagenUrl = await GuardarArchivoImagenAsync(productoId, archivo);
                }

                await _catalogoProductosServicio.ActualizarProductoAsync(
                    productoId,
                    Vm.Form.Nombre,
                    Vm.Form.Precio,
                    Vm.Form.CategoriaId,
                    Vm.Form.Descripcion,
                    imagenUrl,
                    Vm.Form.TiempoPreparacionMin);
                ToastSuccess = "Producto actualizado correctamente.";
            }
            else
            {
                var creado = await _catalogoProductosServicio.CrearProductoAsync(
                    Vm.Form.Nombre,
                    Vm.Form.Precio,
                    Vm.Form.CategoriaId,
                    Vm.Form.Descripcion,
                    null,
                    Vm.Form.TiempoPreparacionMin ?? 5);
                productoId = creado.Id;

                if (archivo is not null && archivo.Length > 0)
                {
                    imagenUrl = await GuardarArchivoImagenAsync(productoId, archivo);
                    await _catalogoProductosServicio.ActualizarProductoAsync(
                        productoId,
                        creado.Nombre,
                        creado.Precio,
                        creado.CategoriaId,
                        creado.Descripcion,
                        imagenUrl,
                        creado.TiempoPreparacionMin);
                }

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

    public async Task<IActionResult> OnPostEliminarFotoAsync(Guid id)
    {
        SetUiContext();
        try
        {
            var producto = (await _catalogoProductosServicio.ListarProductosAsync())
                .FirstOrDefault(p => p.Id == id);

            if (producto is not null)
            {
                EliminarArchivoImagen(id);
                await _catalogoProductosServicio.ActualizarProductoAsync(
                    id,
                    producto.Nombre,
                    producto.Precio,
                    producto.CategoriaId,
                    producto.Descripcion,
                    null,
                    producto.TiempoPreparacionMin);
                ToastSuccess = "Foto eliminada correctamente.";
            }
        }
        catch (Exception ex)
        {
            ToastError = ex.Message;
        }

        return RedirectToPage(new { Buscar, CategoriaId, Estado });
    }

    private async Task<string> GuardarArchivoImagenAsync(Guid productoId, IFormFile archivo)
    {
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "productos");
        Directory.CreateDirectory(uploadsDir);

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (extension == ".jpeg") extension = ".jpg";

        var filePath = Path.Combine(uploadsDir, $"{productoId}{extension}");

        // Eliminar archivos previos con cualquier extension
        foreach (var ext in ExtensionesPermitidas)
        {
            var previo = Path.Combine(uploadsDir, $"{productoId}{ext}");
            if (System.IO.File.Exists(previo))
            {
                System.IO.File.Delete(previo);
            }
        }

        using var stream = System.IO.File.Create(filePath);
        await archivo.CopyToAsync(stream);

        return $"/uploads/productos/{productoId}{extension}";
    }

    private void EliminarArchivoImagen(Guid productoId)
    {
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "productos");
        foreach (var ext in ExtensionesPermitidas)
        {
            var filePath = Path.Combine(uploadsDir, $"{productoId}{ext}");
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
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
