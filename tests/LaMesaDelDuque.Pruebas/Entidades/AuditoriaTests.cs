using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Pruebas.Entidades;

public class AuditoriaTests
{
    private static Usuario CrearUsuario() =>
        new("admin01", "admin@lmd.local", "$2a$12$hashValido", "Admin", new Rol("admin"));

    [Fact]
    public void CrearAuditoria_CuandoDatosSonValidos_DebeCrearInstancia()
    {
        var registroId = Guid.NewGuid();
        var audit = new Auditoria("pedido", registroId, "INSERT", CrearUsuario(), datosNuevos: "{\"id\":1}");

        Assert.Equal("pedido", audit.TablaAfectada);
        Assert.Equal("INSERT", audit.Accion);
        Assert.Equal(registroId, audit.RegistroId);
    }

    [Fact]
    public void CrearAuditoria_CuandoAccionEsInvalida_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() =>
            new Auditoria("pedido", Guid.NewGuid(), "SELECT", CrearUsuario()));

        Assert.Contains("acción", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CrearAuditoria_CuandoTablaEsVacia_DebeLanzarExcepcion()
    {
        var ex = Assert.Throws<ReglaDominioException>(() =>
            new Auditoria(" ", Guid.NewGuid(), "DELETE", CrearUsuario()));

        Assert.Contains("tabla", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
