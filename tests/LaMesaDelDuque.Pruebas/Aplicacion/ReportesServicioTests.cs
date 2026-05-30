using LaMesaDelDuque.Aplicacion.Servicios;
using LaMesaDelDuque.Dominio.Entidades;
using LaMesaDelDuque.Dominio.Enumeraciones;

namespace LaMesaDelDuque.Pruebas.Aplicacion;

public sealed class ReportesServicioTests
{
    [Fact]
    public void DescribirServicio_DomicilioSinMesa_DebeMostrarDomicilio()
    {
        var pedido = new Pedido(
            TipoServicio.Domicilio,
            mesa: null,
            nombreClienteEntrega: "Cliente Demo",
            telefonoEntrega: "809-555-0000",
            direccionEntrega: "Calle Principal #1");

        var descripcion = ReportesServicio.DescribirServicio(pedido);

        Assert.Equal("Domicilio", descripcion);
    }

    [Fact]
    public void DescribirServicio_ParaLlevarSinMesa_DebeMostrarParaLlevar()
    {
        var pedido = new Pedido(TipoServicio.ParaLlevar);

        var descripcion = ReportesServicio.DescribirServicio(pedido);

        Assert.Equal("Para llevar", descripcion);
    }

    [Fact]
    public void DescribirServicio_ComerAquiConMesa_DebeMostrarNumeroMesa()
    {
        var mesa = new Mesa(12, 4);
        var pedido = new Pedido(TipoServicio.ComerAqui, mesa);

        var descripcion = ReportesServicio.DescribirServicio(pedido);

        Assert.Equal("Mesa 12", descripcion);
    }
}
