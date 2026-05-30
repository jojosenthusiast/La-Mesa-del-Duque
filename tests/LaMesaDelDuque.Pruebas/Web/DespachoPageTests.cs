using LaMesaDelDuque.Pruebas.Calidad;
using LaMesaDelDuque.Web.Pages.Operaciones.Despacho;

namespace LaMesaDelDuque.Pruebas.Web;

public class DespachoPageTests
{
    [Fact]
    public void CalcularMinutosEsperaDespacho_CuandoReferenciaEsLocal_NoInflaPorZonaHoraria()
    {
        var ahoraUtc = DateTime.UtcNow;
        var referenciaLocal = ahoraUtc.ToLocalTime();

        var minutos = IndexModel.CalcularMinutosEsperaDespacho(referenciaLocal, ahoraUtc);

        Assert.InRange(minutos, 0, 1);
    }

    [Fact]
    public void DespachoMarkup_DebeUsarReferenciaDeListoAntesQueFechaCreacion()
    {
        var source = File.ReadAllText(Path.Combine(ProjectPaths.RepoRoot, "src", "LaMesaDelDuque.Web", "Pages", "Operaciones", "Despacho", "Index.cshtml"));

        Assert.Contains("pedido.FechaListoDespacho ?? pedido.FechaCreacion", source);
        Assert.Contains("CalcularMinutosEsperaDespacho", source);
        Assert.DoesNotContain("DateTime.UtcNow - pedido.FechaCreacion", source);
    }
}
