using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace LaMesaDelDuque.Pruebas.Aplicacion;

internal static class TestHttpContextAccessor
{
    public static IHttpContextAccessor ConUsuarioAutenticado(Guid? usuarioId = null, params string[] roles)
    {
        var claimsList = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, (usuarioId ?? Guid.NewGuid()).ToString())
        };

        foreach (var role in roles)
            claimsList.Add(new Claim(ClaimTypes.Role, role));

        var claims = new ClaimsPrincipal(new ClaimsIdentity(claimsList, "TestAuth"));

        return new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = claims }
        };
    }
}
