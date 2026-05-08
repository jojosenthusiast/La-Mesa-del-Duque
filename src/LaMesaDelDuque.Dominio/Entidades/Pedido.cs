using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class Pedido
{
    private readonly List<DetallePedido> _detalles = [];

    public Guid Id { get; private set; }
    public Mesa Mesa { get; private set; }
    public EstadoPedido Estado { get; private set; }
    public IReadOnlyList<DetallePedido> Detalles => _detalles.AsReadOnly();
    public decimal Total => _detalles.Sum(d => d.Subtotal);

    private Pedido()
    {
        Mesa = null!;
    }

    public Pedido(Mesa mesa)
    {
        if (mesa is null)
            throw new ReglaDominioException("El pedido debe estar asociado a una mesa.");

        Id = Guid.NewGuid();
        Mesa = mesa;
        Estado = EstadoPedido.Abierto;
    }

    public void AgregarDetalle(DetallePedido detalle)
    {
        if (detalle is null)
            throw new ReglaDominioException("El detalle no puede ser nulo.");

        if (Estado == EstadoPedido.Cerrado)
            throw new ReglaDominioException("No se pueden agregar detalles a un pedido cerrado.");

        _detalles.Add(detalle);
    }

    public void Cerrar()
    {
        if (Estado == EstadoPedido.Cerrado)
            throw new ReglaDominioException("El pedido ya está cerrado.");

        if (_detalles.Count == 0)
            throw new ReglaDominioException("No se puede cerrar un pedido sin detalles.");

        Estado = EstadoPedido.Cerrado;
    }

    public void Cancelar()
    {
        if (Estado == EstadoPedido.Cancelado)
            throw new ReglaDominioException("El pedido ya está cancelado.");

        if (Estado == EstadoPedido.Cerrado)
            throw new ReglaDominioException("No se puede cancelar un pedido cerrado.");

        Estado = EstadoPedido.Cancelado;
    }

    public void EliminarDetalle(Guid detalleId)
    {
        if (Estado == EstadoPedido.Cerrado)
            throw new ReglaDominioException("No se pueden modificar los detalles de un pedido cerrado.");

        if (Estado == EstadoPedido.Cancelado)
            throw new ReglaDominioException("No se pueden modificar los detalles de un pedido cancelado.");

        if (_detalles.Count == 1)
            throw new ReglaDominioException("No se puede eliminar el único detalle del pedido. Use cancelar en su lugar.");

        var detalle = _detalles.FirstOrDefault(d => d.Id == detalleId)
            ?? throw new ReglaDominioException("El detalle especificado no pertenece a este pedido.");

        _detalles.Remove(detalle);
    }
}
