using LaMesaDelDuque.Dominio.Enumeraciones;
using LaMesaDelDuque.Dominio.Excepciones;

namespace LaMesaDelDuque.Dominio.Entidades;

public class DescuentoAplicado
{
    public Guid Id { get; private set; }
    public Guid PedidoId { get; private set; }
    public Guid? DetallePedidoId { get; private set; }
    public Guid MotivoId { get; private set; }
    public MotivoDescuento Motivo { get; private set; } = null!;
    public string TipoDescuento { get; private set; }
    public decimal Valor { get; private set; }
    public decimal MontoAplicado { get; private set; }
    public EstadoDescuento Estado { get; private set; }
    public Guid UsuarioSolicitaId { get; private set; }
    public Guid? UsuarioAutorizaId { get; private set; }
    public DateTime FechaSolicitud { get; private set; }
    public DateTime? FechaResolucion { get; private set; }
    public string? NotaAutorizador { get; private set; }

    private DescuentoAplicado() { TipoDescuento = string.Empty; }

    public DescuentoAplicado(
        Guid pedidoId,
        Guid motivoId,
        string tipoDescuento,
        decimal valor,
        decimal montoAplicado,
        Guid usuarioSolicitaId,
        Guid? detallePedidoId = null)
    {
        if (valor <= 0) throw new ReglaDominioException("El valor del descuento debe ser positivo.");
        if (montoAplicado < 0) throw new ReglaDominioException("El monto aplicado no puede ser negativo.");
        Id = Guid.NewGuid();
        PedidoId = pedidoId;
        DetallePedidoId = detallePedidoId;
        MotivoId = motivoId;
        TipoDescuento = tipoDescuento;
        Valor = valor;
        MontoAplicado = montoAplicado;
        Estado = EstadoDescuento.Pendiente;
        UsuarioSolicitaId = usuarioSolicitaId;
        FechaSolicitud = DateTime.UtcNow;
    }

    public void Aprobar(Guid usuarioAutorizaId, string? nota = null)
    {
        if (Estado != EstadoDescuento.Pendiente)
            throw new ReglaDominioException("Solo se puede aprobar un descuento pendiente.");
        Estado = EstadoDescuento.Aprobado;
        UsuarioAutorizaId = usuarioAutorizaId;
        FechaResolucion = DateTime.UtcNow;
        NotaAutorizador = nota?.Trim();
    }

    public void Rechazar(Guid usuarioAutorizaId, string? nota = null)
    {
        if (Estado != EstadoDescuento.Pendiente)
            throw new ReglaDominioException("Solo se puede rechazar un descuento pendiente.");
        Estado = EstadoDescuento.Rechazado;
        UsuarioAutorizaId = usuarioAutorizaId;
        FechaResolucion = DateTime.UtcNow;
        NotaAutorizador = nota?.Trim();
    }
}
