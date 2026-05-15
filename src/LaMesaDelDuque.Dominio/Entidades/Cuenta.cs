using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class Cuenta
{
    private readonly List<CuentaDetalle> _detallesAsignados = [];
    private decimal _totalBase;

    public Guid Id { get; private set; }
    public Guid PedidoId { get; private set; }
    public int Numero { get; private set; }  // 1, 2, 3...
    public decimal Total => _detallesAsignados.Count > 0
        ? _detallesAsignados.Sum(d => d.Subtotal)
        : _totalBase;
    public decimal PropinaMonto { get; private set; }
    public MetodoPago? MetodoPago { get; private set; }
    public EstadoCuenta Estado { get; private set; }
    public DateTime? FechaPago { get; private set; }
    public byte[] Version { get; private set; } = Array.Empty<byte>();
    public IReadOnlyList<CuentaDetalle> DetallesAsignados => _detallesAsignados.AsReadOnly();

    private Cuenta() { } // EF Core

    public Cuenta(Guid pedidoId, int numero) : this()
    {
        if (numero < 1) throw new ReglaDominioException("El número de cuenta debe ser mayor que cero.");
        Id = Guid.NewGuid();
        PedidoId = pedidoId;
        Numero = numero;
        Estado = EstadoCuenta.Abierta;
    }

    public void EstablecerTotalBase(decimal total)
    {
        if (total < 0) throw new ReglaDominioException("El total de la cuenta no puede ser negativo.");
        _totalBase = total;
    }

    public void AsignarItem(DetallePedido detalle, int cantidad = 1)
    {
        if (detalle is null)
            throw new ReglaDominioException("El detalle no puede ser nulo.");

        if (cantidad <= 0)
            throw new ReglaDominioException("La cantidad asignada debe ser mayor que cero.");

        if (cantidad > detalle.Cantidad)
            throw new ReglaDominioException("La cantidad asignada no puede exceder la cantidad del detalle.");

        var yaAsignada = _detallesAsignados
            .Where(d => d.DetallePedidoId == detalle.Id)
            .Sum(d => d.CantidadAsignada);

        if (yaAsignada + cantidad > detalle.Cantidad)
            throw new ReglaDominioException("La cantidad total asignada no puede exceder la cantidad del detalle.");

        _detallesAsignados.Add(new CuentaDetalle(Id, detalle.Id, cantidad, detalle.PrecioUnitario));
    }

    public void Pagar(MetodoPago metodo, decimal propinaMonto = 0)
    {
        if (Estado == EstadoCuenta.Pagada)
            throw new ReglaDominioException("Esta cuenta ya fue pagada.");
        if (propinaMonto < 0)
            throw new ReglaDominioException("La propina no puede ser negativa.");
        MetodoPago = metodo;
        PropinaMonto = propinaMonto;
        Estado = EstadoCuenta.Pagada;
        FechaPago = DateTime.UtcNow;
    }
}
