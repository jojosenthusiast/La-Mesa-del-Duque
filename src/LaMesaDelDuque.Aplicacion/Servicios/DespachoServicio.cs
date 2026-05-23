using LaMesaDelDuque.Dominio.Excepciones;
using LaMesaDelDuque.Dominio.Repositorios;

namespace LaMesaDelDuque.Aplicacion.Servicios;

public sealed class DespachoServicio : IDespachoServicio
{
    private readonly IUnidadDeTrabajo _uot;

    public DespachoServicio(IUnidadDeTrabajo uot)
    {
        _uot = uot;
    }

    public async Task DespacharPedidoAsync(Guid pedidoId, CancellationToken cancelacion = default)
    {
        var pedido = await _uot.Pedidos.ObtenerConDetallesParaActualizarAsync(pedidoId, cancelacion)
            ?? throw new ReglaDominioException("Pedido no encontrado.");

        pedido.MarcarDespachado();

        if (pedido.Mesa is not null)
        {
            var mesa = await _uot.Mesas.ObtenerParaActualizarAsync(pedido.Mesa.Id, cancelacion)
                ?? throw new ReglaDominioException("Mesa no encontrada.");

            mesa.Liberar();
        }

        await _uot.GuardarCambiosAsync(cancelacion);
    }
}
