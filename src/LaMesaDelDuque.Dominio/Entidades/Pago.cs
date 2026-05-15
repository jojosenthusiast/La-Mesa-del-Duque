using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class Pago
{
    public Guid Id { get; private set; }
    public Guid CuentaId { get; private set; }
    public decimal Monto { get; private set; }
    public decimal PropinaMonto { get; private set; }
    public MetodoPago Metodo { get; private set; }
    public DateTime FechaPago { get; private set; }
    public Guid UsuarioId { get; private set; }

    private Pago() { }

    public Pago(Guid cuentaId, decimal monto, MetodoPago metodo, decimal propinaMonto = 0, Guid usuarioId = default) : this()
    {
        if (monto <= 0) throw new ReglaDominioException("El monto del pago debe ser mayor que cero.");
        if (propinaMonto < 0) throw new ReglaDominioException("La propina no puede ser negativa.");
        Id = Guid.NewGuid();
        CuentaId = cuentaId;
        Monto = monto;
        Metodo = metodo;
        PropinaMonto = propinaMonto;
        UsuarioId = usuarioId;
        FechaPago = DateTime.UtcNow;
    }
}
