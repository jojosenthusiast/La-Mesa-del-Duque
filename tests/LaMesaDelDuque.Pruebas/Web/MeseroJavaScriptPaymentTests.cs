using LaMesaDelDuque.Pruebas.Calidad;

namespace LaMesaDelDuque.Pruebas.Web;

public class MeseroJavaScriptPaymentTests
{
    [Fact]
    public void MeseroJs_NoDebeExponerFlujoDePagoDirecto()
    {
        var scriptPath = Path.Combine(ProjectPaths.RepoRoot, "src", "LaMesaDelDuque.Web", "wwwroot", "js", "mesero.js");
        var source = File.ReadAllText(scriptPath);

        Assert.DoesNotContain("PagarJson", source, StringComparison.Ordinal);
        Assert.DoesNotContain("abrirReferenciaPago", source, StringComparison.Ordinal);
        Assert.DoesNotContain("lmd-mesero-payment-ref", source, StringComparison.Ordinal);
        Assert.DoesNotContain("referencia: referencia", source, StringComparison.Ordinal);
    }
}
