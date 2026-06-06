using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Pruebas.Entidades;

public class CierreDiaTests
{
    private static Usuario CrearUsuario() =>
        new("cajero01", "cajero@lmd.local", "$2a$12$hashValido", "Cajero", new Rol("cajero"));

    [Fact]
    public void CrearCierreDia_CuandoDatosSonValidos_DebeCrearInstancia()
    {
        var fecha = new DateOnly(2026, 5, 10);
        var cierre = new CierreDia(fecha, 1500m, 900m, 600m, 30, 2, 50m, CrearUsuario());

        Assert.NotEqual(Guid.Empty, cierre.Id);
        Assert.Equal(fecha, cierre.Fecha);
        Assert.Equal(1500m, cierre.TotalVentas);
        Assert.Equal(30, cierre.TotalPedidos);
    }

    [Fact]
    public void CrearCierreDia_CuandoUsuarioEsNulo_DebeAceptar()
    {
        var cierre = new CierreDia(new DateOnly(2026, 5, 10), 0m, 0m, 0m, 0, 0, 0m, null);
        Assert.Equal(Guid.Empty, cierre.UsuarioId);
    }

    [Fact]
    public void CrearCierreDia_CuandoTotalVentasEsNegativo_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() =>
            new CierreDia(new DateOnly(2026, 5, 10), -100m, 0m, 0m, 0, 0, 0m, CrearUsuario()));

        Assert.Contains("ventas", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
