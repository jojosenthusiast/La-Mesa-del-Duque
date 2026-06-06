using LaMesaDelDuque.Dominio.Enumeraciones;
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

        var mesaId = pedido.Mesa?.Id;

        if (pedido.Estado == EstadoPedido.Pagado)
            pedido.MarcarListo();

        pedido.MarcarDespachado();

        if (mesaId.HasValue && !await TieneOtrosPedidosActivosEnMesaAsync(mesaId.Value, pedido.Id, cancelacion))
        {
            var mesa = await _uot.Mesas.ObtenerParaActualizarAsync(mesaId.Value, cancelacion)
                ?? throw new ReglaDominioException("Mesa no encontrada.");

            mesa.Liberar();
            var minutosGracia = await _uot.ObtenerPeriodoGraciaMinutosAsync(cancelacion);
            mesa.IniciarGracia(minutosGracia);
        }

        await _uot.GuardarCambiosAsync(cancelacion);
    }

    private async Task<bool> TieneOtrosPedidosActivosEnMesaAsync(Guid mesaId, Guid pedidoActualId, CancellationToken cancelacion)
    {
        var pedidosMesa = await _uot.Pedidos.ObtenerPorMesaAsync(mesaId, cancelacion);
        return pedidosMesa.Any(p => p.Id != pedidoActualId && MantieneMesaOcupada(p.Estado));
    }

    private static bool MantieneMesaOcupada(EstadoPedido estado) =>
        estado is EstadoPedido.Pendiente
            or EstadoPedido.EnPreparacion
            or EstadoPedido.EnCobro
            or EstadoPedido.Pagado
            or EstadoPedido.Listo;
}
