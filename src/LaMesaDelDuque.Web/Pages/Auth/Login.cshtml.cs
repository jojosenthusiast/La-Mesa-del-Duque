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
    private static readonly ConcurrentDictionary<string, (int Count, DateTimeOffset FirstAttemptUtc)> IntentosFallidos = new();
    private const int MaxIntentosPorMinuto = 5;
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
        var llaveIntento = $"{HttpContext.Connection.RemoteIpAddress}:{Username?.Trim().ToLowerInvariant()}";
        if (IntentosFallidos.TryGetValue(llaveIntento, out var intento)
            && intento.FirstAttemptUtc > DateTimeOffset.UtcNow.AddMinutes(-1)
            && intento.Count >= MaxIntentosPorMinuto)
        {
            MensajeError = "Demasiados intentos fallidos. Espere un minuto antes de reintentar.";
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

        return RedirectToPage("/Index");
    }

    private static void RegistrarIntentoFallido(string llaveIntento)
    {
        IntentosFallidos.AddOrUpdate(
            llaveIntento,
            _ => (1, DateTimeOffset.UtcNow),
            (_, actual) => actual.FirstAttemptUtc <= DateTimeOffset.UtcNow.AddMinutes(-1)
                ? (1, DateTimeOffset.UtcNow)
                : (actual.Count + 1, actual.FirstAttemptUtc));
    }
}
