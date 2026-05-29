namespace LaMesaDelDuque.Aplicacion.Dtos;

public class DevolucionPagoDto
{
    public Guid Id { get; set; }
    public Guid PagoOriginalId { get; set; }
    public decimal MontoDevuelto { get; set; }
    public string MetodoDevolucion { get; set; } = string.Empty;
    public string MotivoDevolucion { get; set; } = string.Empty;
    public Guid UsuarioSolicitaId { get; set; }
    public Guid UsuarioAutorizaId { get; set; }
    public DateTime FechaHora { get; set; }
    public bool StockReintegrado { get; set; }
}
