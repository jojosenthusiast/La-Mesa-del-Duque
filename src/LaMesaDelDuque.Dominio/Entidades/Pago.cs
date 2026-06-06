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
    public string? ReferenciaPos { get; private set; }

    private Pago() { }

    public Pago(Guid cuentaId, decimal monto, MetodoPago metodo, decimal propinaMonto = 0, Guid usuarioId = default, string? referenciaPos = null) : this()
    {
        if (monto <= 0) throw new ReglaDominioException("El monto del pago debe ser mayor que cero.");
        if (propinaMonto < 0) throw new ReglaDominioException("La propina no puede ser negativa.");
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("El usuario del pago es obligatorio para auditoria.", nameof(usuarioId));
        if (metodo == MetodoPago.Tarjeta && string.IsNullOrWhiteSpace(referenciaPos))
            throw new ReglaDominioException("El número de referencia del dataphone es obligatorio para pagos con tarjeta.");

        Id = Guid.NewGuid();
        CuentaId = cuentaId;
        Monto = monto;
        Metodo = metodo;
        PropinaMonto = propinaMonto;
        UsuarioId = usuarioId;
        ReferenciaPos = referenciaPos?.Trim();
        FechaPago = DateTime.UtcNow;
    }
}
