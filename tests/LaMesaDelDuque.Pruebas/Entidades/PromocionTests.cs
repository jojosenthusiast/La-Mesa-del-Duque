using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Pruebas.Entidades;

public class PromocionTests
{
    [Fact]
    public void CrearPromocion_CuandoDatosSonValidos_DebeCrearInstancia()
    {
        var inicio = new DateOnly(2026, 1, 1);
        var fin = new DateOnly(2026, 12, 31);
        var promo = new Promocion("Happy Hour", "porcentaje", 15m, inicio, fin);

        Assert.Equal("Happy Hour", promo.Nombre);
        Assert.Equal("porcentaje", promo.TipoDescuento);
        Assert.Equal(15m, promo.ValorDescuento);
        Assert.True(promo.Activo);
        Assert.NotEqual(Guid.Empty, promo.Id);
    }

    [Fact]
    public void CrearPromocion_CuandoTipoDescuentoEsInvalido_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() =>
            new Promocion("Promo", "gratis", 10m, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)));

        Assert.Contains("tipo", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearPromocion_CuandoValorEsNegativo_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() =>
            new Promocion("Promo", "fijo", -5m, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)));

        Assert.Contains("valor", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
