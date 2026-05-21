using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class Pedido
{
    private readonly List<DetallePedido> _detalles = [];
    private readonly List<Cuenta> _cuentas = [];

    public Guid Id { get; private set; }
    public DateTime FechaCreacion { get; private set; } = DateTime.UtcNow;
    public TipoServicio TipoServicio { get; private set; }
    public Mesa? Mesa { get; private set; }
    public EstadoPedido Estado { get; private set; }
    public IReadOnlyList<DetallePedido> Detalles => _detalles.AsReadOnly();
    public IReadOnlyList<Cuenta> Cuentas => _cuentas.AsReadOnly();
    public decimal Total => _detalles.Sum(d => d.Subtotal);
    public bool EstaPagadoCompletamente => Cuentas.Count > 0 && Cuentas.All(c => c.Estado == EstadoCuenta.Pagada);

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

    public void MarcarEnCobro()
    {
        if (Estado != EstadoPedido.EnPreparacion)
            throw new ReglaDominioException("Solo se puede marcar en cobro un pedido en preparación.");

        Estado = EstadoPedido.EnCobro;
    }

    public void MarcarComoPagado()
    {
        if (Estado == EstadoPedido.Pagado)
            throw new ReglaDominioException("El pedido ya está pagado.");

        if (Estado == EstadoPedido.Cancelado)
            throw new ReglaDominioException("No se puede pagar un pedido cancelado.");

        if (_detalles.Count == 0)
            throw new ReglaDominioException("No se puede pagar un pedido sin detalles.");

        if (Cuentas.Count > 0 && !EstaPagadoCompletamente)
            throw new ReglaDominioException("No se puede marcar como pagado un pedido con cuentas pendientes.");

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

    public void AgregarCuenta(Cuenta cuenta)
    {
        if (cuenta is null)
            throw new ReglaDominioException("La cuenta no puede ser nula.");

        if (cuenta.PedidoId != Id)
            throw new ReglaDominioException("La cuenta no pertenece a este pedido.");

        _cuentas.Add(cuenta);
    }

    public IReadOnlyList<Cuenta> CrearCuentas(int cantidad)
    {
        if (cantidad < 1)
            throw new ReglaDominioException("La cantidad de cuentas debe ser al menos 1.");

        if (_detalles.Count == 0)
            throw new ReglaDominioException("No se pueden crear cuentas para un pedido sin detalles.");

        _cuentas.Clear();

        decimal totalPorCuenta = Math.Floor((Total / cantidad) * 100) / 100;
        decimal sumaAsignada = 0;

        for (int i = 1; i <= cantidad; i++)
        {
            decimal monto = i == cantidad ? Total - sumaAsignada : totalPorCuenta;
            var cuenta = new Cuenta(Id, i);
            cuenta.EstablecerTotalBase(monto);
            _cuentas.Add(cuenta);
            sumaAsignada += monto;
        }

        return Cuentas;
    }

    public IReadOnlyList<Cuenta> CrearCuentasConItems(Dictionary<int, List<(DetallePedido detalle, int cantidad)>> asignaciones)
    {
        if (asignaciones is null || asignaciones.Count < 2)
            throw new ReglaDominioException("Se requieren al menos 2 cuentas para dividir por items.");

        if (_detalles.Count == 0)
            throw new ReglaDominioException("No se pueden crear cuentas para un pedido sin detalles.");

        var detalleIds = _detalles.Select(d => d.Id).ToHashSet();
        foreach (var kvp in asignaciones)
        {
            foreach (var (detalle, cantidad) in kvp.Value)
            {
                if (detalle is null)
                    throw new ReglaDominioException("El detalle no puede ser nulo.");

                if (!detalleIds.Contains(detalle.Id))
                    throw new ReglaDominioException("El detalle especificado no pertenece a este pedido.");

                if (cantidad <= 0)
                    throw new ReglaDominioException("La cantidad asignada debe ser mayor que cero.");

                if (cantidad > detalle.Cantidad)
                    throw new ReglaDominioException("La cantidad asignada no puede exceder la cantidad del detalle.");
            }
        }

        foreach (var detalle in _detalles)
        {
            var totalAsignada = asignaciones
                .SelectMany(a => a.Value)
                .Where(x => x.detalle.Id == detalle.Id)
                .Sum(x => x.cantidad);

            if (totalAsignada > detalle.Cantidad)
                throw new ReglaDominioException($"La cantidad total asignada del item '{detalle.Producto.Nombre}' excede la cantidad pedida.");
        }

        _cuentas.Clear();

        foreach (var kvp in asignaciones.OrderBy(a => a.Key))
        {
            var cuenta = new Cuenta(Id, kvp.Key);
            foreach (var (detalle, cantidad) in kvp.Value)
            {
                cuenta.AsignarItem(detalle, cantidad);
            }
            _cuentas.Add(cuenta);
        }

        return Cuentas;
    }
}
