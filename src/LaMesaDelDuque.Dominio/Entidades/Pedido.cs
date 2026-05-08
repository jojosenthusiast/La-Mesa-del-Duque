using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class Pedido
{
    private readonly List<DetallePedido> _detalles = [];

    public Guid Id { get; private set; }
    public TipoServicio TipoServicio { get; private set; }
    public Mesa? Mesa { get; private set; }
    public EstadoPedido Estado { get; private set; }
    public IReadOnlyList<DetallePedido> Detalles => _detalles.AsReadOnly();
    public decimal Total => _detalles.Sum(d => d.Subtotal);

    private Pedido()
    {
    }

    public Pedido(TipoServicio tipoServicio, Mesa? mesa = null)
    {
        if (tipoServicio == TipoServicio.ParaLlevar && mesa is not null)
            throw new ReglaDominioException("Un pedido para llevar no puede tener mesa asignada.");

        Id = Guid.NewGuid();
        TipoServicio = tipoServicio;
        Mesa = mesa;
        Estado = EstadoPedido.Pendiente;
    }

    public void MarcarEnPreparacion()
    {
        if (Estado != EstadoPedido.Pendiente)
            throw new ReglaDominioException("Solo se puede marcar en preparación un pedido pendiente.");

        if (_detalles.Count == 0)
            throw new ReglaDominioException("No se puede marcar en preparación un pedido sin detalles.");

        Estado = EstadoPedido.EnPreparacion;
    }

    public void MarcarComoPagado()
    {
        if (Estado == EstadoPedido.Pagado)
            throw new ReglaDominioException("El pedido ya está pagado.");

        if (Estado == EstadoPedido.Cancelado)
            throw new ReglaDominioException("No se puede pagar un pedido cancelado.");

        if (_detalles.Count == 0)
            throw new ReglaDominioException("No se puede pagar un pedido sin detalles.");

        Estado = EstadoPedido.Pagado;
    }

    public void Cancelar()
    {
        if (Estado == EstadoPedido.Cancelado)
            throw new ReglaDominioException("El pedido ya está cancelado.");

        if (Estado == EstadoPedido.Pagado)
            throw new ReglaDominioException("No se puede cancelar un pedido pagado.");

        Estado = EstadoPedido.Cancelado;
    }

    public void AgregarDetalle(DetallePedido detalle)
    {
        if (detalle is null)
            throw new ReglaDominioException("El detalle no puede ser nulo.");

        if (Estado == EstadoPedido.Pagado)
            throw new ReglaDominioException("No se pueden agregar detalles a un pedido pagado.");

        if (Estado == EstadoPedido.Cancelado)
            throw new ReglaDominioException("No se pueden agregar detalles a un pedido cancelado.");

        _detalles.Add(detalle);
    }

    public void EliminarDetalle(Guid detalleId)
    {
        if (Estado == EstadoPedido.Pagado)
            throw new ReglaDominioException("No se pueden modificar los detalles de un pedido pagado.");

        if (Estado == EstadoPedido.Cancelado)
            throw new ReglaDominioException("No se pueden modificar los detalles de un pedido cancelado.");

        if (_detalles.Count == 1)
            throw new ReglaDominioException("No se puede eliminar el único detalle del pedido. Use cancelar en su lugar.");

        var detalle = _detalles.FirstOrDefault(d => d.Id == detalleId)
            ?? throw new ReglaDominioException("El detalle especificado no pertenece a este pedido.");

        _detalles.Remove(detalle);
    }
}
