using LaMesaDelDuque.Pruebas.Calidad;

namespace LaMesaDelDuque.Pruebas.Web;

public class CriticalVendorAssetsTests
{
    private static readonly string[] ForbiddenRuntimeHosts =
    [
        "cdn.jsdelivr.net",
        "cdnjs.cloudflare.com",
        "fonts.googleapis.com",
        "fonts.gstatic.com",
        "unpkg.com"
    ];

    private static readonly string[] RequiredVendorAssets =
    [
        "src/LaMesaDelDuque.Web/wwwroot/lib/microsoft/signalr/dist/browser/signalr.min.js",
        "src/LaMesaDelDuque.Web/wwwroot/lib/chart.js/chart.umd.min.js",
        "src/LaMesaDelDuque.Web/wwwroot/lib/bootstrap-icons/font/bootstrap-icons.css",
        "src/LaMesaDelDuque.Web/wwwroot/lib/bootstrap-icons/font/fonts/bootstrap-icons.woff2",
        "src/LaMesaDelDuque.Web/wwwroot/lib/lucide-static/icons/alert-triangle.svg",
        "src/LaMesaDelDuque.Web/wwwroot/lib/lucide-static/icons/arrow-left.svg",
        "src/LaMesaDelDuque.Web/wwwroot/lib/lucide-static/icons/check.svg",
        "src/LaMesaDelDuque.Web/wwwroot/lib/lucide-static/icons/package.svg"
    ];

    [Fact]
    public void RuntimeSources_NoDebenDependerDeCdnsExternos()
    {
        var violations = RuntimeSourceFiles()
            .SelectMany(file => ForbiddenRuntimeHosts
                .Where(host => File.ReadAllText(file).Contains(host, StringComparison.OrdinalIgnoreCase))
                .Select(host => $"{Relative(file)} contiene {host}"))
            .ToList();

        Assert.True(violations.Count == 0,
            "Los assets críticos deben servirse localmente, no desde CDNs públicos:" + Environment.NewLine +
            string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void CriticalVendorAssets_DebenEstarVersionadosLocalmente()
    {
        var missing = RequiredVendorAssets
            .Select(path => path.Replace('/', Path.DirectorySeparatorChar))
            .Where(path => !File.Exists(Path.Combine(ProjectPaths.RepoRoot, path)))
            .ToList();

        Assert.True(missing.Count == 0,
            "Faltan assets vendor críticos bajo wwwroot/lib:" + Environment.NewLine +
            string.Join(Environment.NewLine, missing));
    }

    [Fact]
    public void SecurityHeaders_NoDebenPermitirHostsCdnLuegoDeLocalizarAssets()
    {
        var source = File.ReadAllText(Path.Combine(
            ProjectPaths.RepoRoot,
            "src",
            "LaMesaDelDuque.Web",
            "Seguridad",
            "SecurityHeadersMiddleware.cs"));

        foreach (var host in ForbiddenRuntimeHosts)
        {
            Assert.DoesNotContain(host, source, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static IEnumerable<string> RuntimeSourceFiles()
    {
        var webRoot = Path.Combine(ProjectPaths.RepoRoot, "src", "LaMesaDelDuque.Web");

        foreach (var file in Directory.EnumerateFiles(Path.Combine(webRoot, "Pages"), "*.cshtml", SearchOption.AllDirectories))
        {
            yield return file;
        }

        foreach (var file in Directory.EnumerateFiles(Path.Combine(webRoot, "wwwroot", "js"), "*.js", SearchOption.AllDirectories))
        {
            yield return file;
        }

        foreach (var file in Directory.EnumerateFiles(Path.Combine(webRoot, "Seguridad"), "*.cs", SearchOption.AllDirectories))
        {
            yield return file;
        }
    }

    private static string Relative(string path) =>
        Path.GetRelativePath(ProjectPaths.RepoRoot, path);
}
