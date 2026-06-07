namespace LaMesaDelDuque.Aplicacion.Dtos;

public class PedidoDto
{
    public Guid Id { get; set; }
    public string TipoServicio { get; set; } = string.Empty;
    public Guid? MesaId { get; set; }
    public int? MesaNumero { get; set; }
    public Guid? MeseroAsignadoId { get; set; }
    public string? ClienteDeliveryNombre { get; set; }
    public string? ClienteDeliveryTelefono { get; set; }
    public string? ClienteDeliveryDireccion { get; set; }
    public string? ClienteDeliveryReferencia { get; set; }
    public string? ClienteDeliveryNotas { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaListoDespacho { get; set; }
    public List<DetallePedidoDto> Detalles { get; set; } = [];

    // Datos logísticos para pedidos a domicilio. Se exponen en DTO para que Caja
    // pueda configurar el envío y el repartidor tenga una vista liviana.
    public Guid? RepartidorId { get; set; }
    public string? DireccionEntrega { get; set; }
    public string? TelefonoCliente { get; set; }
    public DateTime? AsignadoEn { get; set; }
    public DateTime? EntregadoEn { get; set; }
}
