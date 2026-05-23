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

    [BindProperty]
    public MermaFormVm MermaForm { get; set; } = new();

    public IndexModel(IInventarioServicio inv, IMermaServicio merma) { _inv = inv; _merma = merma; }

    public async Task OnGetAsync()
    {
        Ingredientes = await _inv.ListarIngredientesAsync();
        Proveedores = await _inv.ListarProveedoresAsync();
        Mermas = await _merma.ObtenerMermasDelDiaAsync();
    }

    public async Task<IActionResult> OnPostRegistrarMermaAsync()
    {
        if (!ModelState.IsValid)
        {
            Ingredientes = await _inv.ListarIngredientesAsync();
            Proveedores = await _inv.ListarProveedoresAsync();
            Mermas = await _merma.ObtenerMermasDelDiaAsync();
            ToastError = "Revisá los campos del formulario.";
            return Page();
        }

        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
        {
            ToastError = "No se pudo identificar el usuario autenticado.";
            return RedirectToPage(null, "mermas");
        }

        try
        {
            await _merma.RegistrarMermaAsync(new RegistrarMermaRequest
            {
                IngredienteId = MermaForm.IngredienteId,
                Cantidad = MermaForm.Cantidad,
                Tipo = MermaForm.Tipo,
                Lote = MermaForm.Lote,
                Notas = MermaForm.Notas
            }, usuarioId);
            ToastSuccess = "Merma registrada. Stock actualizado.";
        }
        catch (InvalidOperationException ex)
        {
            ToastError = ex.Message;
        }
        catch (ArgumentException ex)
        {
            ToastError = ex.Message;
        }

        return RedirectToPage(null, "mermas");
    }
}

public class MermaFormVm
{
    [Required(ErrorMessage = "Seleccioná un ingrediente.")]
    public Guid IngredienteId { get; set; }

    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(0.001, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public decimal Cantidad { get; set; }

    public TipoMerma Tipo { get; set; } = TipoMerma.Otro;

    [MaxLength(50)]
    public string? Lote { get; set; }

    [MaxLength(500)]
    public string? Notas { get; set; }
}
