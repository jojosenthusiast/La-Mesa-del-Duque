using LaMesaDelDuque.Dominio.Enumeraciones;

namespace LaMesaDelDuque.Aplicacion.Dtos;

public class CuentaDto
{
    public Guid Id { get; set; }
    public Guid PedidoId { get; set; }
    public int Numero { get; set; }
    public decimal Total { get; set; }
    public decimal PropinaMonto { get; set; }
    public string? MetodoPago { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime? FechaPago { get; set; }
    public List<CuentaDetalleDto> Detalles { get; set; } = [];
}
