namespace LaMesaDelDuque.Aplicacion.Dtos;

public class CuentaDetalleDto
{
    public Guid Id { get; set; }
    public Guid DetallePedidoId { get; set; }
    public int CantidadAsignada { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
