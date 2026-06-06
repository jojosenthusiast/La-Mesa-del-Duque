using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Pruebas.Entidades;

public class ProveedorTests
{
    [Fact]
    public void CrearProveedor_CuandoDatosSonValidos_DebeCrearInstancia()
    {
        var proveedor = new Proveedor("Distribuidora Central", "0614-250890-102-3", "Carlos", "7777-1111", "compras@proveedor.com", "Santa Ana");

        Assert.Equal("Distribuidora Central", proveedor.Nombre);
        Assert.Equal("0614-250890-102-3", proveedor.Nit);
        Assert.True(proveedor.Activo);
        Assert.NotEqual(Guid.Empty, proveedor.Id);
    }

    [Fact]
    public void CrearProveedor_CuandoNombreONitSonInvalidos_DebeLanzarExcepcion()
    {
        var exNombre = Assert.Throws<ReglaDominioException>(() =>
            new Proveedor(" ", "0614-250890-102-3"));

        Assert.Contains("nombre", exNombre.Message, StringComparison.OrdinalIgnoreCase);

        var exNit = Assert.Throws<ReglaDominioException>(() =>
            new Proveedor("Distribuidora Central", " "));

        Assert.Contains("nit", exNit.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearProveedor_CuandoNitTieneFormatoInvalido_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() =>
            new Proveedor("Distribuidora Central", "NIT-INVALIDO"));

        Assert.Contains("formato", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
