using Microsoft.Extensions.Primitives;

namespace LaMesaDelDuque.Web.Seguridad;

public sealed class SecurityHeadersMiddleware
{
    private const string ContentSecurityPolicy = "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "font-src 'self' data:; " +
        "img-src 'self' data: https:; " +
        "connect-src 'self'; " +
        "base-uri 'self'; " +
        "form-action 'self'; " +
        "frame-ancestors 'self'; " +
        "object-src 'none'";

    private static readonly IReadOnlyDictionary<string, StringValues> Headers = new Dictionary<string, StringValues>
    {
        ["X-Content-Type-Options"] = "nosniff",
        ["Referrer-Policy"] = "strict-origin-when-cross-origin",
        ["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()",
        ["Content-Security-Policy"] = ContentSecurityPolicy,
        ["X-Frame-Options"] = "SAMEORIGIN"
    };

    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        foreach (var (name, value) in Headers)
        {
            context.Response.Headers.TryAdd(name, value);
        }

        await _next(context);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseLaMesaSecurityHeaders(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
