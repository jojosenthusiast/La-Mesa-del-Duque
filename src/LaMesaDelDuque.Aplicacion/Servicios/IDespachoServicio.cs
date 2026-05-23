namespace LaMesaDelDuque.Aplicacion.Servicios;

public interface IDespachoServicio
{
    Task DespacharPedidoAsync(Guid pedidoId, CancellationToken cancelacion = default);
}
