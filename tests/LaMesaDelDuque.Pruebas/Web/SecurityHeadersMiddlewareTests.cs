using LaMesaDelDuque.Web.Seguridad;
using LaMesaDelDuque.Pruebas.Calidad;
using Microsoft.AspNetCore.Http;

namespace LaMesaDelDuque.Pruebas.Web;

public class SecurityHeadersMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_DebeAgregarCabecerasDeSeguridadBase()
    {
        var context = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal("nosniff", context.Response.Headers["X-Content-Type-Options"]);
        Assert.Equal("strict-origin-when-cross-origin", context.Response.Headers["Referrer-Policy"]);
        Assert.Equal("camera=(), microphone=(), geolocation=(), payment=()", context.Response.Headers["Permissions-Policy"]);

        var csp = context.Response.Headers.ContentSecurityPolicy.ToString();
        Assert.Contains("default-src 'self'", csp);
        Assert.Contains("object-src 'none'", csp);
        Assert.Contains("frame-ancestors 'self'", csp);
        Assert.Contains("connect-src 'self'", csp);
    }

    [Fact]
    public async Task InvokeAsync_NoDebeSobrescribirCabecerasExplicitas()
    {
        var context = new DefaultHttpContext();
        context.Response.Headers["Referrer-Policy"] = "no-referrer";
        context.Response.Headers.ContentSecurityPolicy = "default-src 'none'";
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal("no-referrer", context.Response.Headers["Referrer-Policy"]);
        Assert.Equal("default-src 'none'", context.Response.Headers.ContentSecurityPolicy);
    }

    [Fact]
    public void Program_DebeRegistrarSecurityHeadersAntesDeArchivosEstaticos()
    {
        var programPath = Path.Combine(ProjectPaths.RepoRoot, "src", "LaMesaDelDuque.Web", "Program.cs");
        var source = File.ReadAllText(programPath);

        var securityIndex = source.IndexOf("UseLaMesaSecurityHeaders", StringComparison.Ordinal);
        var staticFilesIndex = source.IndexOf("UseStaticFiles", StringComparison.Ordinal);

        Assert.True(securityIndex >= 0, "Program.cs debe registrar UseLaMesaSecurityHeaders().");
        Assert.True(securityIndex < staticFilesIndex, "Las cabeceras de seguridad deben aplicarse antes de UseStaticFiles().");
    }
}
