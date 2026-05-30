namespace LaMesaDelDuque.Dominio.Modelos;

public class MetricasOperativasDto
{
    public decimal VentasHoy { get; set; }
    public int MesasActivas { get; set; }
    public int TotalMesas { get; set; }
    public decimal TurnoverRate { get; set; }
    public int PedidosExcedenSLA { get; set; }
}
