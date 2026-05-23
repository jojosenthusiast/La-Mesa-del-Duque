using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Enumeraciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace LaMesaDelDuque.Web.Pages.Operaciones.Inventario;

[Authorize(Roles = "Administrador,Encargado")]
public class IndexModel : PageModel
{
    private readonly IInventarioServicio _inv;
    private readonly IMermaServicio _merma;

    public List<IngredienteDto> Ingredientes { get; set; } = [];
    public List<ProveedorDto> Proveedores { get; set; } = [];
    public List<MermaDiariaDto> Mermas { get; set; } = [];

    [TempData] public string? ToastSuccess { get; set; }
    [TempData] public string? ToastError { get; set; }

    [BindProperty] public IngredienteFormVm IngForm { get; set; } = new();
    [BindProperty] public ProveedorFormVm PrvForm { get; set; } = new();
    [BindProperty] public MermaFormVm MermaForm { get; set; } = new();
    [BindProperty] public AjusteStockVm StockForm { get; set; } = new();

    public IndexModel(IInventarioServicio inv, IMermaServicio merma) { _inv = inv; _merma = merma; }

    public async Task OnGetAsync()
    {
        Ingredientes = await _inv.ListarIngredientesAsync();
        Proveedores = await _inv.ListarProveedoresAsync();
        Mermas = await _merma.ObtenerMermasDelDiaAsync();
    }

    public async Task<IActionResult> OnPostCrearIngredienteAsync()
    {
        try
        {
            await _inv.CrearIngredienteAsync(new GuardarIngredienteRequest
            {
                Nombre = IngForm.Nombre, UnidadMedida = IngForm.UnidadMedida,
                StockActual = IngForm.StockActual, StockMinimo = IngForm.StockMinimo,
                CostoUnitario = IngForm.CostoUnitario, ProveedorId = IngForm.ProveedorId
            });
            ToastSuccess = $"Ingrediente \"{IngForm.Nombre}\" creado.";
        }
        catch (Exception ex) { ToastError = ex.Message; }
        return RedirectToPage(new { tab = "ingredientes" });
    }

    public async Task<IActionResult> OnPostActualizarIngredienteAsync()
    {
        try
        {
            await _inv.ActualizarIngredienteAsync(IngForm.Id!.Value, new GuardarIngredienteRequest
            {
                Nombre = IngForm.Nombre, UnidadMedida = IngForm.UnidadMedida,
                StockActual = IngForm.StockActual, StockMinimo = IngForm.StockMinimo,
                CostoUnitario = IngForm.CostoUnitario, ProveedorId = IngForm.ProveedorId
            });
            ToastSuccess = $"Ingrediente \"{IngForm.Nombre}\" actualizado.";
        }
        catch (Exception ex) { ToastError = ex.Message; }
        return RedirectToPage(new { tab = "ingredientes" });
    }

    public async Task<IActionResult> OnPostAjustarStockAsync()
    {
        try { await _inv.AjustarStockAsync(StockForm.IngredienteId, StockForm.NuevoStock); ToastSuccess = "Stock ajustado."; }
        catch (Exception ex) { ToastError = ex.Message; }
        return RedirectToPage(new { tab = "ingredientes" });
    }

    public async Task<IActionResult> OnPostToggleIngredienteAsync(Guid id)
    {
        try { await _inv.ToggleIngredienteActivoAsync(id); }
        catch (Exception ex) { ToastError = ex.Message; }
        return RedirectToPage(new { tab = "ingredientes" });
    }

    public async Task<IActionResult> OnPostCrearProveedorAsync()
    {
        try
        {
            await _inv.CrearProveedorAsync(new GuardarProveedorRequest
            {
                Nombre = PrvForm.Nombre, Nit = PrvForm.Nit,
                Contacto = PrvForm.Contacto, Telefono = PrvForm.Telefono,
                Email = PrvForm.Email, Direccion = PrvForm.Direccion
            });
            ToastSuccess = $"Proveedor \"{PrvForm.Nombre}\" creado.";
        }
        catch (Exception ex) { ToastError = ex.Message; }
        return RedirectToPage(new { tab = "proveedores" });
    }

    public async Task<IActionResult> OnPostActualizarProveedorAsync()
    {
        try
        {
            await _inv.ActualizarProveedorAsync(PrvForm.Id!.Value, new GuardarProveedorRequest
            {
                Nombre = PrvForm.Nombre, Nit = PrvForm.Nit,
                Contacto = PrvForm.Contacto, Telefono = PrvForm.Telefono,
                Email = PrvForm.Email, Direccion = PrvForm.Direccion
            });
            ToastSuccess = $"Proveedor \"{PrvForm.Nombre}\" actualizado.";
        }
        catch (Exception ex) { ToastError = ex.Message; }
        return RedirectToPage(new { tab = "proveedores" });
    }

    public async Task<IActionResult> OnPostToggleProveedorAsync(Guid id)
    {
        try { await _inv.ToggleProveedorActivoAsync(id); }
        catch (Exception ex) { ToastError = ex.Message; }
        return RedirectToPage(new { tab = "proveedores" });
    }

    public async Task<JsonResult> OnGetDetallePrv(Guid id)
    {
        try { return new JsonResult(await _inv.ObtenerProveedorAsync(id)); }
        catch { return new JsonResult(null); }
    }

    public async Task<IActionResult> OnPostRegistrarMermaAsync()
    {
        var uid = GetUsuarioId();
        if (uid == Guid.Empty) { ToastError = "No se pudo identificar el usuario."; return RedirectToPage(new { tab = "mermas" }); }
        try
        {
            await _merma.RegistrarMermaAsync(new RegistrarMermaRequest
            {
                IngredienteId = MermaForm.IngredienteId, Cantidad = MermaForm.Cantidad,
                Tipo = MermaForm.Tipo, Notas = MermaForm.Notas, Lote = MermaForm.Lote
            }, uid);
            ToastSuccess = "Merma registrada.";
        }
        catch (Exception ex) { ToastError = ex.Message; }
        return RedirectToPage(new { tab = "mermas" });
    }

    private Guid GetUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}

public class IngredienteFormVm
{
    public Guid? Id { get; set; }
    [Required(ErrorMessage = "El nombre es obligatorio.")] [MaxLength(150)] public string Nombre { get; set; } = "";
    [Required(ErrorMessage = "La unidad de medida es obligatoria.")] [MaxLength(20)] public string UnidadMedida { get; set; } = "";
    [Range(0, double.MaxValue)] public decimal StockActual { get; set; }
    [Range(0, double.MaxValue)] public decimal StockMinimo { get; set; }
    [Range(0, double.MaxValue)] public decimal CostoUnitario { get; set; }
    public Guid? ProveedorId { get; set; }
}

public class ProveedorFormVm
{
    public Guid? Id { get; set; }
    [Required(ErrorMessage = "El nombre es obligatorio.")] [MaxLength(200)] public string Nombre { get; set; } = "";
    [Required(ErrorMessage = "El NIT es obligatorio.")] [RegularExpression(@"^\d{4}-\d{6}-\d{3}-\d$", ErrorMessage = "Formato: 0000-000000-000-0")] public string Nit { get; set; } = "";
    [MaxLength(150)] public string? Contacto { get; set; }
    [MaxLength(20)] public string? Telefono { get; set; }
    [MaxLength(150)] [EmailAddress(ErrorMessage = "Email invalido.")] public string? Email { get; set; }
    [MaxLength(300)] public string? Direccion { get; set; }
}

public class MermaFormVm
{
    [Required] public Guid IngredienteId { get; set; }
    [Range(0.001, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")] public decimal Cantidad { get; set; }
    public TipoMerma Tipo { get; set; } = TipoMerma.Otro;
    [MaxLength(50)] public string? Lote { get; set; }
    [MaxLength(500)] public string? Notas { get; set; }
}

public class AjusteStockVm
{
    public Guid IngredienteId { get; set; }
    [Range(0, double.MaxValue)] public decimal NuevoStock { get; set; }
}
