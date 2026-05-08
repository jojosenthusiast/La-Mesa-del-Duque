using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Pruebas.Entidades;

public class SeguridadYAuditoriaEntidadTests
{
    [Fact]
    public void CrearRol_CuandoNombreExcede50_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() => new Rol(new string('R', 51)));

        Assert.Contains("50", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearUsuario_CuandoEmailTieneFormatoInvalido_DebeLanzarExcepcion()
    {
        var rol = new Rol("cajero");

        var ex = Assert.Throws<ReglaDominioException>(() =>
            new Usuario("cajero01", "email-invalido", "$2a$12$hashValidoSegunFlujo", "Cajero Uno", rol));

        Assert.Contains("email", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearProductoPrecioHistorial_CuandoRazonEsVacia_DebeLanzarExcepcion()
    {
        var categoria = new CategoriaProducto("Comidas");
        var producto = new Producto("Hamburguesa", 6.5m, categoria);
        var usuario = new Usuario("admin01", "admin@lmd.local", "$2a$12$hashValidoSegunFlujo", "Administrador General", new Rol("admin"));

        var ex = Assert.Throws<ReglaDominioException>(() =>
            new ProductoPrecioHistorial(producto, 5m, 6m, " ", usuario));

        Assert.Contains("razón", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearPedidoEstadoLog_CuandoEstadosSonValidos_DebeCrearRegistro()
    {
        var usuario = new Usuario("admin02", null, "$2a$12$hashValidoSegunFlujo", "Administrador Dos", new Rol("admin"));
        var log = new PedidoEstadoLog(Guid.NewGuid(), "pendiente", "en_preparacion", usuario, "Inicio de preparación");

        Assert.Equal("pendiente", log.EstadoAnterior);
        Assert.Equal("en_preparacion", log.EstadoNuevo);
        Assert.Equal(usuario.Id, log.UsuarioId);
    }
}
