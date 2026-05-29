using LaMesaDelDuque.Pruebas.Calidad;

namespace LaMesaDelDuque.Pruebas.Web;

public class MeseroJavaScriptPaymentTests
{
    [Fact]
    public void MeseroJs_DebeCapturarReferenciaAntesDePagosNoEfectivo()
    {
        var scriptPath = Path.Combine(ProjectPaths.RepoRoot, "src", "LaMesaDelDuque.Web", "wwwroot", "js", "mesero.js");
        var source = File.ReadAllText(scriptPath);

        Assert.Contains("abrirReferenciaPago(\\'tarjeta\\'", source, StringComparison.Ordinal);
        Assert.Contains("abrirReferenciaPago(\\'qr\\'", source, StringComparison.Ordinal);
        Assert.Contains("lmd-mesero-payment-ref", source, StringComparison.Ordinal);
        Assert.Contains("referencia: referencia", source, StringComparison.Ordinal);
    }
}
