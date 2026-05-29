namespace LaMesaDelDuque.Aplicacion.Dtos;

public class PedidoDto
{
    public Guid Id { get; set; }
    public string TipoServicio { get; set; } = string.Empty;
    public Guid? MesaId { get; set; }
    public int? MesaNumero { get; set; }
    public Guid? MeseroAsignadoId { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime FechaCreacion { get; set; }
    public List<DetallePedidoDto> Detalles { get; set; } = [];
}
