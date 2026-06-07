using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace LaMesaDelDuque.Web.Pages.Operaciones.Inventario;

[Authorize(Roles = "Administrador,Encargado")]
public class IndexModel : PageModel
{
    private const string MensajeErrorInesperado = "Ocurrio un error interno. Intenta nuevamente.";

    private readonly IInventarioServicio _inv;
    private readonly IMermaServicio _merma;
    private readonly ILogger<IndexModel> _logger;

    public List<IngredienteDto> Ingredientes { get; set; } = [];
    public List<ProveedorDto> Proveedores { get; set; } = [];
    public List<MermaDiariaDto> Mermas { get; set; } = [];

    [TempData] public string? ToastSuccess { get; set; }
    [TempData] public string? ToastError { get; set; }

    [BindProperty] public IngredienteFormVm IngForm { get; set; } = new();
    [BindProperty] public ProveedorFormVm PrvForm { get; set; } = new();
    [BindProperty] public MermaFormVm MermaForm { get; set; } = new();
    [BindProperty] public AjusteStockVm StockForm { get; set; } = new();

    public Guid? EditIngredienteId { get; set; }
    public Guid? EditProveedorId { get; set; }

    public IndexModel(IInventarioServicio inv, IMermaServicio merma, ILogger<IndexModel> logger)
    {
        _inv = inv;
        _merma = merma;
        _logger = logger;
    }

    public async Task OnGetAsync(Guid? editIngrediente, Guid? editProveedor)
    {
        Ingredientes = await _inv.ListarIngredientesAsync();
        Proveedores = await _inv.ListarProveedoresAsync();
        Mermas = await _merma.ObtenerMermasDelDiaAsync();

        if (editIngrediente.HasValue)
        {
            EditIngredienteId = editIngrediente.Value;
            var ing = Ingredientes.FirstOrDefault(i => i.Id == editIngrediente.Value);
            if (ing is not null)
            {
                IngForm = new IngredienteFormVm
                {
                    Id = ing.Id, Nombre = ing.Nombre, UnidadMedida = ing.UnidadMedida,
                    StockActual = ing.StockActual, StockMinimo = ing.StockMinimo,
                    CostoUnitario = ing.CostoUnitario, ProveedorId = ing.ProveedorId
                };
            }
        }

        if (editProveedor.HasValue)
        {
            EditProveedorId = editProveedor.Value;
            var prv = Proveedores.FirstOrDefault(p => p.Id == editProveedor.Value);
            if (prv is not null)
            {
                PrvForm = new ProveedorFormVm
                {
                    Id = prv.Id, Nombre = prv.Nombre, Nit = prv.Nit,
                    Contacto = prv.Contacto, Telefono = prv.Telefono,
                    Email = prv.Email, Direccion = prv.Direccion
                };
            }
        }
    }

    public async Task<IActionResult> OnPostCrearIngredienteAsync(string Nombre, decimal StockActual, decimal StockMinimo, string UnidadMedida, decimal CostoUnitario, Guid? ProveedorId)
    {
        try
        {
            await _inv.CrearIngredienteAsync(new GuardarIngredienteRequest { Nombre = Nombre, UnidadMedida = UnidadMedida, StockActual = StockActual, StockMinimo = StockMinimo, CostoUnitario = CostoUnitario, ProveedorId = ProveedorId });
            ToastSuccess = "Ingrediente creado.";
        }
        catch (ReglaDominioException ex) { RegistrarErrorDeNegocio(ex.Message); }
        catch (ArgumentException ex) { RegistrarErrorDeNegocio(ex.Message); }
        catch (Exception ex) { RegistrarErrorInesperado(ex, "crear ingrediente"); }
        return RedirectToPage(new { tab = "ingredientes" });
    }

    public async Task<IActionResult> OnPostEditarIngredienteAsync(Guid Id, string Nombre, decimal StockActual, decimal StockMinimo, string UnidadMedida, decimal CostoUnitario, Guid? ProveedorId)
    {
        try
        {
            await _inv.ActualizarIngredienteAsync(Id, new GuardarIngredienteRequest { Nombre = Nombre, UnidadMedida = UnidadMedida, StockActual = StockActual, StockMinimo = StockMinimo, CostoUnitario = CostoUnitario, ProveedorId = ProveedorId });
            ToastSuccess = "Ingrediente actualizado.";
        }
        catch (ReglaDominioException ex) { RegistrarErrorDeNegocio(ex.Message); }
        catch (ArgumentException ex) { RegistrarErrorDeNegocio(ex.Message); }
        catch (Exception ex) { RegistrarErrorInesperado(ex, "editar ingrediente"); }
        return RedirectToPage(new { tab = "ingredientes" });
    }

    public async Task<IActionResult> OnPostToggleIngredienteAsync(Guid id)
    {
        try
        {
            await _inv.ToggleIngredienteActivoAsync(id);
            ToastSuccess = "Estado del ingrediente cambiado.";
        }
        catch (ReglaDominioException ex) { RegistrarErrorDeNegocio(ex.Message); }
        catch (ArgumentException ex) { RegistrarErrorDeNegocio(ex.Message); }
        catch (Exception ex) { RegistrarErrorInesperado(ex, "cambiar estado del ingrediente"); }
        return RedirectToPage(new { tab = "ingredientes" });
    }

    public async Task<IActionResult> OnPostCrearProveedorAsync(string Nombre, string Nit, string? Contacto, string? Telefono)
    {
        try
        {
            await _inv.CrearProveedorAsync(new GuardarProveedorRequest { Nombre = Nombre, Nit = Nit, Contacto = Contacto, Telefono = Telefono });
            ToastSuccess = "Proveedor creado.";
        }
        catch (ReglaDominioException ex) { RegistrarErrorDeNegocio(ex.Message); }
        catch (ArgumentException ex) { RegistrarErrorDeNegocio(ex.Message); }
        catch (Exception ex) { RegistrarErrorInesperado(ex, "crear proveedor"); }
        return RedirectToPage(new { tab = "proveedores" });
    }

    public async Task<IActionResult> OnPostEditarProveedorAsync(Guid Id, string Nombre, string Nit, string? Contacto, string? Telefono, string? Email, string? Direccion)
    {
        try
        {
            await _inv.ActualizarProveedorAsync(Id, new GuardarProveedorRequest { Nombre = Nombre, Nit = Nit, Contacto = Contacto, Telefono = Telefono, Email = Email, Direccion = Direccion });
            ToastSuccess = "Proveedor actualizado.";
        }
        catch (ReglaDominioException ex) { RegistrarErrorDeNegocio(ex.Message); }
        catch (ArgumentException ex) { RegistrarErrorDeNegocio(ex.Message); }
        catch (Exception ex) { RegistrarErrorInesperado(ex, "editar proveedor"); }
        return RedirectToPage(new { tab = "proveedores" });
    }

    public async Task<IActionResult> OnPostToggleProveedorAsync(Guid id)
    {
        try
        {
            await _inv.ToggleProveedorActivoAsync(id);
            ToastSuccess = "Estado del proveedor cambiado.";
        }
        catch (ReglaDominioException ex) { RegistrarErrorDeNegocio(ex.Message); }
        catch (ArgumentException ex) { RegistrarErrorDeNegocio(ex.Message); }
        catch (Exception ex) { RegistrarErrorInesperado(ex, "cambiar estado del proveedor"); }
        return RedirectToPage(new { tab = "proveedores" });
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
        catch (ReglaDominioException ex) { RegistrarErrorDeNegocio(ex.Message); }
        catch (ArgumentException ex) { RegistrarErrorDeNegocio(ex.Message); }
        catch (InvalidOperationException ex) when (EsErrorOperativoSeguro(ex)) { RegistrarErrorDeNegocio(ex.Message); }
        catch (Exception ex) { RegistrarErrorInesperado(ex, "registrar merma"); }
        return RedirectToPage(new { tab = "mermas" });
    }

    private static bool EsErrorOperativoSeguro(InvalidOperationException ex)
    {
        return ex.Message.StartsWith("No hay cierre de", StringComparison.OrdinalIgnoreCase)
            && ex.Message.Contains("registrar mermas", StringComparison.OrdinalIgnoreCase);
    }

    private void RegistrarErrorInesperado(Exception ex, string accion)
    {
        _logger.LogError(ex, "Error inesperado al {Accion} en inventario.", accion);
        ToastError = MensajeErrorInesperado;
    }

    private void RegistrarErrorDeNegocio(string mensaje)
    {
        ToastError = mensaje;
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
