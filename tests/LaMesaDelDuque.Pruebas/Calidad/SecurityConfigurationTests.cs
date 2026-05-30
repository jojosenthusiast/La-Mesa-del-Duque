using System.Text.RegularExpressions;

namespace LaMesaDelDuque.Pruebas.Calidad;

public class SecurityConfigurationTests
{
    [Fact]
    public void InfraestructuraNoDebeEscribirConnectionStringsEnConsole()
    {
        var path = Path.Combine(ProjectPaths.RepoRoot, "src", "LaMesaDelDuque.Infraestructura", "InyeccionInfraestructura.cs");
        var source = File.ReadAllText(path);

        Assert.DoesNotContain("Console.WriteLine", source);
        Assert.DoesNotContain("cs='", source);
    }

    [Fact]
    public void AppsettingsVersionadosNoDebenContenerPasswordPostgresReal()
    {
        var webProject = Path.Combine(ProjectPaths.RepoRoot, "src", "LaMesaDelDuque.Web");
        var appsettingsFiles = Directory.GetFiles(webProject, "appsettings*.json*", SearchOption.TopDirectoryOnly);
        var failures = new List<string>();

        foreach (var file in appsettingsFiles)
        {
            var content = File.ReadAllText(file);

            foreach (Match match in Regex.Matches(content, "Password\\s*=\\s*([^;\\\"\\s]+)", RegexOptions.IgnoreCase))
            {
                var value = match.Groups[1].Value;
                if (EsPlaceholderSeguro(value))
                {
                    continue;
                }

                failures.Add($"{Path.GetRelativePath(ProjectPaths.RepoRoot, file)} contiene un password real en un connection string.");
            }
        }

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    private static bool EsPlaceholderSeguro(string value) =>
        string.IsNullOrWhiteSpace(value)
        || value.Contains("TU_", StringComparison.OrdinalIgnoreCase)
        || value.Contains("YOUR_", StringComparison.OrdinalIgnoreCase)
        || value.Contains("PLACEHOLDER", StringComparison.OrdinalIgnoreCase)
        || value.Contains("<", StringComparison.OrdinalIgnoreCase);
}
