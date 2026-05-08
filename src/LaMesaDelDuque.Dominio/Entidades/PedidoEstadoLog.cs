using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class PedidoEstadoLog
{
    public Guid Id { get; private set; }
    public Guid PedidoId { get; private set; }
    public string EstadoAnterior { get; private set; }
    public string EstadoNuevo { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; }
    public string? Notas { get; private set; }
    public DateTime FechaCambio { get; private set; }

    private PedidoEstadoLog()
    {
        EstadoAnterior = string.Empty;
        EstadoNuevo = string.Empty;
        Usuario = null!;
    }

    public PedidoEstadoLog(Guid pedidoId, string estadoAnterior, string estadoNuevo, Usuario usuario, string? notas = null)
    {
        if (pedidoId == Guid.Empty)
            throw new ReglaDominioException("El pedido es obligatorio para registrar cambio de estado.");
        if (string.IsNullOrWhiteSpace(estadoAnterior) || estadoAnterior.Trim().Length > 20)
            throw new ReglaDominioException("El estado anterior es obligatorio y no puede exceder 20 caracteres.");
        if (string.IsNullOrWhiteSpace(estadoNuevo) || estadoNuevo.Trim().Length > 20)
            throw new ReglaDominioException("El estado nuevo es obligatorio y no puede exceder 20 caracteres.");
        if (usuario is null)
            throw new ReglaDominioException("El usuario que cambia el estado es obligatorio.");
        if (!string.IsNullOrWhiteSpace(notas) && notas.Trim().Length > 500)
            throw new ReglaDominioException("Las notas no pueden exceder 500 caracteres.");

        Id = Guid.NewGuid();
        PedidoId = pedidoId;
        EstadoAnterior = estadoAnterior.Trim();
        EstadoNuevo = estadoNuevo.Trim();
        Usuario = usuario;
        UsuarioId = usuario.Id;
        Notas = string.IsNullOrWhiteSpace(notas) ? null : notas.Trim();
        FechaCambio = DateTime.UtcNow;
    }
}
