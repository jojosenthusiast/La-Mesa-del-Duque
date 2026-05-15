namespace LaMesaDelDuque.Aplicacion.Dtos;

public class DetalleCreacionDto
{
    public Guid ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public string? Notas { get; set; }
}
