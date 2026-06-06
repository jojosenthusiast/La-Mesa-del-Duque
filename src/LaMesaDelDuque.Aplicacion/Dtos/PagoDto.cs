using LaMesaDelDuque.Dominio.Enumeraciones;

namespace LaMesaDelDuque.Aplicacion.Dtos;

public class PagoDto
{
    public Guid Id { get; set; }
    public Guid CuentaId { get; set; }
    public decimal Monto { get; set; }
    public decimal PropinaMonto { get; set; }
    public string Metodo { get; set; } = string.Empty;
    public DateTime FechaPago { get; set; }
}
