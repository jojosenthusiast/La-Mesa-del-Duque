namespace LaMesaDelDuque.Aplicacion.Dtos;

public class DescuentoAplicadoDto
{
    public Guid Id { get; set; }
    public Guid PedidoId { get; set; }
    public Guid? DetallePedidoId { get; set; }
    public MotivoDescuentoDto Motivo { get; set; } = null!;
    public string TipoDescuento { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal MontoAplicado { get; set; }
    public string Estado { get; set; } = string.Empty;
    public Guid UsuarioSolicitaId { get; set; }
    public Guid? UsuarioAutorizaId { get; set; }
    public DateTime FechaSolicitud { get; set; }
    public DateTime? FechaResolucion { get; set; }
    public string? NotaAutorizador { get; set; }
}
