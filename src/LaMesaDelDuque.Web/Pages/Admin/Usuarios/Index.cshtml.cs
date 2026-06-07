using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Web.Models.Operaciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages.Admin.Usuarios;

[Authorize(Roles = "Administrador")]
public class IndexModel : PageModel
{
    private readonly IUsuariosServicio _usuariosServicio;

    public IndexModel(IUsuariosServicio usuariosServicio)
    {
        _usuariosServicio = usuariosServicio;
    }

    [BindProperty]
    public UsuariosPageVm Vm { get; set; } = new();

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
        if (!ModelState.IsValid)
        {
            await CargarDatosAsync();
            return Page();
        }

        try
        {
            await _usuariosServicio.CrearUsuarioAsync(
                Vm.Form.Username,
                Vm.Form.Email,
                Vm.Form.Password,
                Vm.Form.NombreCompleto,
                Vm.Form.RolId);

            ToastSuccess = "Usuario creado correctamente.";
            return RedirectToPage();
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (InvalidOperationException ex)
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
            await _usuariosServicio.DesactivarUsuarioAsync(id);
            ToastSuccess = "Usuario desactivado correctamente.";
        }
        catch (ArgumentException ex)
        {
            ToastError = ex.Message;
        }

        return RedirectToPage();
    }

    private async Task CargarDatosAsync()
    {
        static bool RolVisible(string rol) => rol is not "Bodega" and not "SuperAdmin" and not "Gerente" and not "Mesero";

        Vm.Usuarios = (await _usuariosServicio.ListarUsuariosAsync())
            .Where(u => RolVisible(u.RolNombre) && u.Username is not "beatriz" and not "super" and not "luciana" and not "maria")
            .ToList();
        Vm.Roles = (await _usuariosServicio.ListarRolesAsync())
            .Where(r => RolVisible(r.Nombre))
            .ToList();
    }

    private void SetUiContext()
    {
        if (ViewData is not null)
        {
            ViewData["ActiveTab"] = "Usuarios";
        }
    }
}
