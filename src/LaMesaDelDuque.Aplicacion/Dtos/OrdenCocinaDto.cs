namespace LaMesaDelDuque.Aplicacion.Dtos;

public class OrdenCocinaDto
{
    public Guid Id { get; set; }
    public Guid PedidoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public string Estacion { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime HoraRecibido { get; set; }
    public int? MesaNumero { get; set; }
    public string? TipoServicio { get; set; }
    public int MinutosTranscurridos => (int)(DateTime.UtcNow - HoraRecibido).TotalMinutes;
}
