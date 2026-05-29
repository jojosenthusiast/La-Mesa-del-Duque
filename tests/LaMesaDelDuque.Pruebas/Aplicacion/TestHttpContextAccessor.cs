using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace LaMesaDelDuque.Pruebas.Aplicacion;

internal static class TestHttpContextAccessor
{
    public static IHttpContextAccessor ConUsuarioAutenticado(Guid? usuarioId = null)
    {
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, (usuarioId ?? Guid.NewGuid()).ToString())
        }, "TestAuth"));

        return new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = claims }
        };
    }
}
