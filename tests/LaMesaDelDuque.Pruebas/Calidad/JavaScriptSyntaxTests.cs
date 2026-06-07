using System.ComponentModel;
using System.Diagnostics;

namespace LaMesaDelDuque.Pruebas.Calidad;

public class JavaScriptSyntaxTests
{
    [Fact]
    public void ArchivosJavaScriptDelWebRootDebenParsearConNode()
    {
        var jsRoot = Path.Combine(ProjectPaths.RepoRoot, "src", "LaMesaDelDuque.Web", "wwwroot", "js");
        var files = Directory.GetFiles(jsRoot, "*.js", SearchOption.AllDirectories)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();

        Assert.NotEmpty(files);

        if (!NodeEstaDisponible())
        {
            return;
        }

        var failures = new List<string>();

        foreach (var file in files)
        {
            var result = RunNodeCheck(file);
            if (result.ExitCode != 0)
            {
                failures.Add($"{Path.GetRelativePath(ProjectPaths.RepoRoot, file)}:{Environment.NewLine}{result.Output}");
            }
        }

        Assert.True(failures.Count == 0, string.Join($"{Environment.NewLine}{Environment.NewLine}", failures));
    }

    private static bool NodeEstaDisponible()
    {
        try
        {
            var startInfo = new ProcessStartInfo("node")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            startInfo.ArgumentList.Add("--version");

            using var process = Process.Start(startInfo);

            return process is not null && process.WaitForExit(5_000) && process.ExitCode == 0;
        }
        catch (Win32Exception)
        {
            return false;
        }
    }

    private static (int ExitCode, string Output) RunNodeCheck(string file)
    {
        var startInfo = new ProcessStartInfo("node")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        startInfo.ArgumentList.Add("--check");
        startInfo.ArgumentList.Add(file);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("No se pudo iniciar node para validar JavaScript.");

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();

        if (!process.WaitForExit(10_000))
        {
            process.Kill(entireProcessTree: true);
            return (-1, $"Timeout validando {file}");
        }

        return (process.ExitCode, string.Concat(stdout, stderr).Trim());
    }
}
