namespace LaMesaDelDuque.Aplicacion.Dtos;

public class DetallePedidoDto
{
    public Guid Id { get; set; }
    public Guid ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal PrecioOriginal { get; set; }
    public decimal DescuentoAplicado { get; set; }
    public string? PromocionNombre { get; set; }
    public decimal Subtotal { get; set; }
    public string? Notas { get; set; }
    public string? ModificacionesJson { get; set; }
}
