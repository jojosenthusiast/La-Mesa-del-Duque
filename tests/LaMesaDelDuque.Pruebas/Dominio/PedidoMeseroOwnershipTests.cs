using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Pruebas.Dominio;

public class PedidoMeseroOwnershipTests
{
    [Fact]
    public void AsignarMesero_PedidoConMesa_DebeGuardarMeseroAsignado()
    {
        var pedido = new Pedido(TipoServicio.ComerAqui, new Mesa(1, 4));
        var meseroId = Guid.NewGuid();

        pedido.AsignarMesero(meseroId);

        Assert.Equal(meseroId, pedido.MeseroAsignadoId);
    }

    [Fact]
    public void AsignarMesero_ConIdVacio_DebeRechazar()
    {
        var pedido = new Pedido(TipoServicio.ComerAqui, new Mesa(2, 4));

        var excepcion = Assert.Throws<ReglaDominioException>(() => pedido.AsignarMesero(Guid.Empty));

        Assert.Contains("mesero", excepcion.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AsignarMesero_PedidoSinMesa_DebeRechazar()
    {
        var pedido = new Pedido(TipoServicio.ParaLlevar);

        var excepcion = Assert.Throws<ReglaDominioException>(() => pedido.AsignarMesero(Guid.NewGuid()));

        Assert.Contains("mesa", excepcion.Message, StringComparison.OrdinalIgnoreCase);
    }
}
