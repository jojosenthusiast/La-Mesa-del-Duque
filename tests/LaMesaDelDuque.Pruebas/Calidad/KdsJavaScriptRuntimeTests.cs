using System.ComponentModel;
using System.Diagnostics;

namespace LaMesaDelDuque.Pruebas.Calidad;

public class KdsJavaScriptRuntimeTests
{
    [Fact]
    public void CocinaKdsDebeRenderizarOrdenesInicialesSinInyectarHtml()
    {
        if (!NodeEstaDisponible())
        {
            return;
        }

        var script = Path.Combine(ProjectPaths.RepoRoot, "tests", "LaMesaDelDuque.Pruebas", "Calidad", "kds-runtime-smoke.js");
        var kdsJavaScript = Path.Combine(ProjectPaths.RepoRoot, "src", "LaMesaDelDuque.Web", "wwwroot", "js", "cocina-kds.js");

        var resultado = RunNode(script, kdsJavaScript);

        Assert.True(resultado.ExitCode == 0, resultado.Output);
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

    private static (int ExitCode, string Output) RunNode(string script, string kdsJavaScript)
    {
        var startInfo = new ProcessStartInfo("node")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = ProjectPaths.RepoRoot
        };
        startInfo.ArgumentList.Add(script);
        startInfo.ArgumentList.Add(kdsJavaScript);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("No se pudo iniciar node para validar el runtime del KDS.");

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();

        if (!process.WaitForExit(10_000))
        {
            process.Kill(entireProcessTree: true);
            return (-1, $"Timeout ejecutando {script}");
        }

        return (process.ExitCode, string.Concat(stdout, stderr).Trim());
    }
}
