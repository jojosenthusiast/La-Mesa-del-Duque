using System.ComponentModel.DataAnnotations;
using LaMesaDelDuque.Aplicacion.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages.Auth;

public class RegisterModel : PageModel
{
    private static readonly IReadOnlyDictionary<string, string> CodigosRol = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["MESERO-789"] = "Mesero",
        ["CAJERO-567"] = "Cajero",
        ["COCINA-456"] = "Cocinero",
        ["ENCARGADO-321"] = "Encargado",
        ["GERENTE-890"] = "Gerente"
    };

    private readonly IUsuariosServicio _usuariosServicio;

    public RegisterModel(IUsuariosServicio usuariosServicio)
    {
        _usuariosServicio = usuariosServicio;
    }

    [BindProperty]
    public RegistroInput Input { get; set; } = new();

    public string? MensajeError { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var clave = (Input.ClaveRol ?? string.Empty).Trim();
        if (!CodigosRol.TryGetValue(clave, out var rolNombre))
        {
            MensajeError = "Clave de rol inválida. Solicita la clave correcta al encargado o administrador.";
            return Page();
        }

        var roles = await _usuariosServicio.ListarRolesAsync();
        var rol = roles.FirstOrDefault(r => string.Equals(r.Nombre, rolNombre, StringComparison.OrdinalIgnoreCase));
        if (rol is null)
        {
            MensajeError = $"No existe el rol '{rolNombre}' en la base de datos.";
            return Page();
        }

        try
        {
            await _usuariosServicio.CrearUsuarioAsync(
                Input.Username.Trim(),
                string.IsNullOrWhiteSpace(Input.Email) ? null : Input.Email.Trim(),
                Input.Password,
                Input.NombreCompleto.Trim(),
                rol.Id);

            TempData["RegistroExitoso"] = $"Usuario creado como {rolNombre}. Ya puedes iniciar sesión.";
            return RedirectToPage("/Auth/Login");
        }
        catch (ArgumentException ex)
        {
            MensajeError = ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            MensajeError = ex.Message;
        }

        return Page();
    }

    public sealed class RegistroInput
    {
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        [StringLength(40, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 40 caracteres.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(120, MinimumLength = 3, ErrorMessage = "El nombre completo debe tener entre 3 y 120 caracteres.")]
        public string NombreCompleto { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirma la contraseña.")]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "La clave de rol es obligatoria.")]
        [Display(Name = "Clave de rol")]
        public string ClaveRol { get; set; } = string.Empty;
    }
}
