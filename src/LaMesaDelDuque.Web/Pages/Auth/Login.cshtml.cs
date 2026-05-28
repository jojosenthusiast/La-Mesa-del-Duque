using System.Security.Claims;
using System.Collections.Concurrent;
using LaMesaDelDuque.Aplicacion.Servicios;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LaMesaDelDuque.Web.Pages.Auth;

public class LoginModel : PageModel
{
    private static readonly ConcurrentDictionary<string, (int Count, DateTimeOffset? LockedUntilUtc)> IntentosFallidos = new();
    private const int MaxIntentosConsecutivos = 5;
    private readonly IUsuariosServicio _usuariosServicio;

    public LoginModel(IUsuariosServicio usuariosServicio)
    {
        _usuariosServicio = usuariosServicio;
    }

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? MensajeError { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var llaveIntento = (Username ?? string.Empty).Trim().ToLowerInvariant();
        if (IntentosFallidos.TryGetValue(llaveIntento, out var intento)
            && intento.LockedUntilUtc.HasValue
            && intento.LockedUntilUtc.Value > DateTimeOffset.UtcNow)
        {
            MensajeError = "Cuenta bloqueada por múltiples intentos fallidos. Intente de nuevo más tarde.";
            return Page();
        }

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            MensajeError = "El usuario y la contraseña son obligatorios.";
            return Page();
        }

        var usuario = await _usuariosServicio.ValidarCredencialesAsync(Username, Password);

        if (usuario is null)
        {
            RegistrarIntentoFallido(llaveIntento);
            MensajeError = "Credenciales inválidas o usuario inactivo.";
            return Page();
        }

        IntentosFallidos.TryRemove(llaveIntento, out _);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Username),
            new(ClaimTypes.GivenName, usuario.NombreCompleto),
            new(ClaimTypes.Role, usuario.RolNombre)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        var destino = usuario.RolNombre switch
        {
            "Administrador" or "Encargado" => "/Admin/Dashboard/Dashboard",
            "Cajero"                        => "/Operaciones/Pedidos/Index",
            "Mesero"                        => "/Operaciones/Pedidos/Index",
            "Cocinero"                      => "/Cocina/KDS",
            _                               => "/Index"
        };
        return RedirectToPage(destino);
    }

    private static void RegistrarIntentoFallido(string llaveIntento)
    {
        IntentosFallidos.AddOrUpdate(
            llaveIntento,
            _ => (1, null),
            (_, actual) =>
            {
                var nuevoConteo = actual.LockedUntilUtc.HasValue && actual.LockedUntilUtc.Value <= DateTimeOffset.UtcNow
                    ? 1
                    : actual.Count + 1;

                return nuevoConteo >= MaxIntentosConsecutivos
                    ? (nuevoConteo, DateTimeOffset.UtcNow.AddMinutes(15))
                    : (nuevoConteo, null);
            });
    }
}
