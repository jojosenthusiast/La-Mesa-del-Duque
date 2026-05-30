namespace LaMesaDelDuque.Aplicacion.Dtos;

public class PedidoDto
{
    public Guid Id { get; set; }
    public string TipoServicio { get; set; } = string.Empty;
    public Guid? MesaId { get; set; }
    public int? MesaNumero { get; set; }
    public Guid? MeseroAsignadoId { get; set; }
    public string? NombreClienteEntrega { get; set; }
    public string? TelefonoEntrega { get; set; }
    public string? DireccionEntrega { get; set; }
    public string? ReferenciaEntrega { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaListoDespacho { get; set; }
    public List<DetallePedidoDto> Detalles { get; set; } = [];
}

public class DatosEntregaDto
{
    public string? NombreCliente { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? Referencia { get; set; }
}
