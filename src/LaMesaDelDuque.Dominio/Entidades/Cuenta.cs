using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class Cuenta
{
    public Guid Id { get; private set; }
    public Guid PedidoId { get; private set; }
    public int Numero { get; private set; }  // 1, 2, 3...
    public decimal Total { get; private set; }
    public decimal PropinaMonto { get; private set; }
    public MetodoPago? MetodoPago { get; private set; }
    public EstadoCuenta Estado { get; private set; }
    public DateTime? FechaPago { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    private Cuenta() { } // EF Core

    public Cuenta(Guid pedidoId, int numero, decimal total) : this()
    {
        if (numero < 1) throw new ReglaDominioException("El número de cuenta debe ser mayor que cero.");
        if (total < 0) throw new ReglaDominioException("El total de la cuenta no puede ser negativo.");
        Id = Guid.NewGuid();
        PedidoId = pedidoId;
        Numero = numero;
        Total = total;
        Estado = EstadoCuenta.Abierta;
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
