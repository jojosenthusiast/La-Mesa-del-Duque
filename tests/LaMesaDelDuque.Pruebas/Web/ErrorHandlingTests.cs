using System.Text.RegularExpressions;

namespace LaMesaDelDuque.Pruebas.Web;

public sealed class ErrorHandlingTests
{
    [Fact]
    public void RazorPageHandlers_NoDevuelvenExceptionMessageCrudo()
    {
        var raiz = EncontrarRaizRepositorio();
        var archivos = Directory.EnumerateFiles(Path.Combine(raiz, "src", "LaMesaDelDuque.Web", "Pages"), "*.cs", SearchOption.AllDirectories);

        var regex = new Regex(@"BadRequest\s*\(\s*ex\.Message\s*\)", RegexOptions.Compiled);
        var ofensores = archivos
            .Where(archivo => regex.IsMatch(File.ReadAllText(archivo)))
            .Select(archivo => Path.GetRelativePath(raiz, archivo))
            .ToArray();

        Assert.True(ofensores.Length == 0, string.Join(Environment.NewLine, ofensores));
    }

    private static string EncontrarRaizRepositorio()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "LaMesaDelDuque.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("No se encontró LaMesaDelDuque.slnx.");
    }
}
