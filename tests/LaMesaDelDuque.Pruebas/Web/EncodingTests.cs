namespace LaMesaDelDuque.Pruebas.Web;

public sealed class EncodingTests
{
    [Fact]
    public void FuentesWeb_NoContienenMojibakeComun()
    {
        var raiz = EncontrarRaizRepositorio();
        var archivos = Directory.EnumerateFiles(Path.Combine(raiz, "src", "LaMesaDelDuque.Web"), "*.*", SearchOption.AllDirectories)
            .Where(ruta => ruta.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                        || ruta.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase)
                        || ruta.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var patrones = new[] { "Ã", "Â¿", "Â¡", "â€”", "â€œ", "â€", "â”" };
        var ofensores = archivos
            .SelectMany(archivo =>
            {
                var contenido = File.ReadAllText(archivo);
                return patrones
                    .Where(contenido.Contains)
                    .Select(patron => $"{Path.GetRelativePath(raiz, archivo)} contiene '{patron}'");
            })
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
