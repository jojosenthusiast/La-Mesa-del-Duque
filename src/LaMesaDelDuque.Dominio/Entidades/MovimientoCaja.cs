using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class MovimientoCaja
{
    public Guid Id { get; private set; }
    public Guid TurnoCajaId { get; private set; }
    public string Tipo { get; private set; }
    public decimal Monto { get; private set; }
    public string Motivo { get; private set; }
    public DateTime FechaHora { get; private set; }
    public Guid UsuarioId { get; private set; }

    private MovimientoCaja() { Tipo = string.Empty; Motivo = string.Empty; }

    public MovimientoCaja(Guid turnoCajaId, string tipo, decimal monto, string motivo, Guid usuarioId)
    {
        if (monto <= 0) throw new ReglaDominioException("El monto del movimiento debe ser positivo.");
        if (string.IsNullOrWhiteSpace(motivo)) throw new ReglaDominioException("El motivo es obligatorio.");
        Id = Guid.NewGuid();
        TurnoCajaId = turnoCajaId;
        Tipo = tipo;
        Monto = monto;
        Motivo = motivo.Trim();
        FechaHora = DateTime.UtcNow;
        UsuarioId = usuarioId;
    }
}
