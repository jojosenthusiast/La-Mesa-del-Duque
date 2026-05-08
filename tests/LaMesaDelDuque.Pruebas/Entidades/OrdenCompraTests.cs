using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Pruebas.Entidades;

public class OrdenCompraTests
{
    private static Usuario CrearUsuario() =>
        new("admin01", "admin@lmd.local", "$2a$12$hashValido", "Admin", new Rol("admin"));

    private static Proveedor CrearProveedor() =>
        new("Distribuidora Central", "0614-250890-102-3");

    [Fact]
    public void CrearOrdenCompra_CuandoDatosSonValidos_DebeCrearInstancia()
    {
        var orden = new OrdenCompra(CrearProveedor(), CrearUsuario());

        Assert.NotEqual(Guid.Empty, orden.Id);
        Assert.Equal("solicitado", orden.Estado);
        Assert.Null(orden.FechaRecepcion);
    }

    [Fact]
    public void CrearOrdenCompra_CuandoProveedorEsNulo_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() =>
            new OrdenCompra(null!, CrearUsuario()));

        Assert.Contains("proveedor", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ActualizarEstado_CuandoEstadoEsInvalido_DebeLanzarExcepcion()
    {
        var orden = new OrdenCompra(CrearProveedor(), CrearUsuario());

        var ex = Assert.Throws<ReglaDominioException>(() =>
            orden.ActualizarEstado("pagado"));

        Assert.Contains("estado", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
